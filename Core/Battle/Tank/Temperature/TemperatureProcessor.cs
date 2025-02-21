using System.Collections.Concurrent;
using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Server.Tank;
using Vint.Core.Structures;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Tank.Temperature;

public class TemperatureProcessor {
    public TemperatureProcessor(BattleTank tank) {
        Tank = tank;
        TemperatureConfig = ConfigManager.GetComponent<TemperatureConfigComponent>(tank.Entities.Tank.TemplateAccessor!.ConfigPath!);
        TickPeriod = TimeSpan.FromMilliseconds(TemperatureConfig.TactPeriodInMs);
    }

    public float Temperature { get; private set; }

    bool IsFrozen => Temperature < 0;
    bool WasFrozen { get; set; }

    TimeSpan TickPeriod { get; }
    TimeSpan RemainingPeriod { get; set; }

    TemperatureConfigComponent TemperatureConfig { get; }
    BattleTank Tank { get; }
    Round Round => Tank.Round;

    ConcurrentQueue<TemperatureAssist> NewAssists { get; } = [];
    ConcurrentList<TemperatureAssist> Assists { get; } = [];

    public async Task Tick(TimeSpan deltaTime) {
        if (Tank.StateManager.CurrentState is not Active)
            return;

        WasFrozen = Temperature < 0;

        UpdateAssistsDuration(deltaTime);
        NormalizeTankTemperature(deltaTime);
        AcceptNewAssists();
        NormalizeAllAssists();

        if (RemainingPeriod > TimeSpan.Zero) {
            RemainingPeriod -= deltaTime;
            return;
        }

        RemainingPeriod += TickPeriod;

        await HeatDamage();
        await UpdateTemperatureAndSpeed();
    }

    public async Task ResetAll() {
        NewAssists.Clear();
        Assists.Clear();

        await UpdateTemperatureAndSpeed();
    }

    public void EnqueueAssist(TemperatureAssist assist) =>
        NewAssists.Enqueue(assist);

    public void ChangeTemperatureConfig(float incrementTemperatureDelta, float decrementTemperatureDelta) {
        TemperatureConfig.AutoIncrementInMs += incrementTemperatureDelta;
        TemperatureConfig.AutoDecrementInMs += decrementTemperatureDelta;
    }

    void UpdateAssistsDuration(TimeSpan deltaTime) {
        foreach (TemperatureAssist assist in Assists.Where(assist => assist.CurrentDuration > TimeSpan.Zero))
            assist.CurrentDuration -= deltaTime;
    }

    async Task HeatDamage() {
        foreach (HeatTemperatureAssist assist in Assists.OfType<HeatTemperatureAssist>()) {
            float value = MathUtils.Map(assist.CurrentDelta, 0, assist.Limit, 0, assist.HeatDamage);

            CalculatedDamage damage = new(default, value, false, false);
            await Round.DamageProcessor.Damage(assist.Tank, Tank, assist.WeaponMarketEntity, assist.WeaponBattleEntity, damage);
        }
    }

    void NormalizeTankTemperature(TimeSpan deltaTime) {
        if (Temperature == 0)
            return;

        TemperatureAssist[] assists = Assists
            .Where(assist => assist.CurrentDuration <= TimeSpan.Zero)
            .ToArray();

        if (assists.Length == 0)
            return;

        float delta = Temperature > 0
            ? TemperatureConfig.AutoDecrementInMs
            : TemperatureConfig.AutoIncrementInMs;

        delta *= (float)deltaTime.TotalMilliseconds;

        while (delta > 0) {
            assists = assists
                .Where(assist => assist.CurrentDelta != 0)
                .ToArray();

            if (assists.Length == 0)
                break;

            float assistDelta = delta / assists.Length;
            float subtracted = 0;

            foreach (TemperatureAssist assist in assists) {
                float subtract = Math.Min(Math.Abs(assist.CurrentDelta), assistDelta);

                assist.Subtract(delta);
                subtracted += subtract;
            }

            delta -= subtracted;
        }
    }

    void AcceptNewAssists() {
        while (NewAssists.TryDequeue(out TemperatureAssist? assist)) {
            TemperatureAssist? existingAssist =
                Assists.FirstOrDefault(a => a.Tank == assist.Tank && a.WeaponMarketEntity == assist.WeaponMarketEntity);

            if (existingAssist != null &&
                existingAssist.CanMerge(assist)) {
                Assists.Remove(existingAssist);
                assist.MergeWith(existingAssist);
            }

            NormalizeWithOpposite(assist);

            if (assist.CurrentDelta == 0 ||
                assist.NormalizeOnly)
                continue;

            Assists.Add(assist);
        }
    }

    void NormalizeAllAssists() {
        float excess = Math.Abs(Assists.Sum(assist => assist.CurrentDelta)) - TemperatureConfig.MaxTemperature;

        if (excess > 0) {
            foreach (TemperatureAssist assist in Assists) {
                float delta = Math.Abs(assist.CurrentDelta);
                float subtract = Math.Min(delta, excess);

                assist.Subtract(subtract);
                excess -= subtract;

                if (excess <= 0)
                    break;
            }
        }

        Assists.RemoveAll(assist => assist.CurrentDelta == 0);
    }

    void NormalizeWithOpposite(TemperatureAssist assist) {
        int sign = Math.Sign(Temperature);

        if (sign == 0 ||
            assist.CurrentDelta == 0 ||
            sign == assist.LimitSign)
            return;

        foreach (TemperatureAssist oppositeAssist in Assists.Where(a => a.LimitSign != assist.LimitSign)) {
            float ownDelta = Math.Abs(assist.CurrentDelta);
            float oppositeDelta = Math.Abs(oppositeAssist.CurrentDelta);
            float subtract = Math.Min(ownDelta, oppositeDelta);

            oppositeAssist.Subtract(subtract);
            assist.Subtract(subtract);

            if (ownDelta == 0)
                break;
        }

        Assists.RemoveAll(a => a.CurrentDelta == 0);
    }

    async Task UpdateTemperatureAndSpeed() {
        float before = Temperature;
        Temperature = Assists.Sum(assist => assist.CurrentDelta);

        if (Math.Abs(Temperature - before) >= float.Epsilon) {
            await Tank.Entities.Tank.ChangeComponent<TemperatureComponent>(component => component.Temperature = Temperature);

            foreach (ITemperatureModule temperatureModule in Tank.Modules.OfType<ITemperatureModule>())
                await temperatureModule.OnTemperatureChanged(before, Temperature, TemperatureConfig.MinTemperature, TemperatureConfig.MaxTemperature);
        }

        if (WasFrozen || IsFrozen)
            await Tank.UpdateSpeed();
    }
}
