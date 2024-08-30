using System.Collections.Concurrent;
using Vint.Core.Battles.Damage;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Server.Tank;
using Vint.Core.ECS.Events.Battle.Movement;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Tank.Temperature;

public class TemperatureProcessor {
    public TemperatureProcessor(BattleTank tank) {
        Tank = tank;
        TemperatureConfig = ConfigManager.GetComponent<TemperatureConfigComponent>(tank.Tank.TemplateAccessor!.ConfigPath!);
        TickPeriod = TimeSpan.FromMilliseconds(TemperatureConfig.TactPeriodInMs);
    }

    public float Temperature { get; private set; }

    bool WasFrozen { get; set; }
    bool Reset { get; set; }

    TimeSpan TickPeriod { get; }
    TimeSpan RemainingPeriod { get; set; }

    TemperatureConfigComponent TemperatureConfig { get; }
    BattleTank Tank { get; }
    Battle Battle => Tank.Battle;

    ConcurrentQueue<TemperatureAssist> NewAssists { get; } = [];
    List<TemperatureAssist> Assists { get; } = [];

    public async Task Tick() {
        if (Reset) {
            Assists.Clear();
            RemainingPeriod = TimeSpan.Zero;
            Reset = false;
            await ResetSpeed();
        }

        if (Tank.StateManager.CurrentState is not Active)
            return;

        UpdateAssistsDuration();

        if (RemainingPeriod > TimeSpan.Zero) {
            RemainingPeriod -= GameServer.DeltaTime;
            return;
        }

        RemainingPeriod = TickPeriod;
        WasFrozen = Temperature < 0;

        await HeatDamage();
        NormalizeTankTemperature();
        AcceptNewAssists();
        NormalizeAllAssists();
        await UpdateTemperatureAndSpeed();
    }

    public void ResetAll() => Reset = true;

    public void EnqueueAssist(TemperatureAssist assist) =>
        NewAssists.Enqueue(assist);

    public void ChangeTemperatureConfig(float incrementTemperatureDelta, float decrementTemperatureDelta) {
        TemperatureConfig.AutoIncrementInMs += incrementTemperatureDelta;
        TemperatureConfig.AutoDecrementInMs += decrementTemperatureDelta;
    }

    void UpdateAssistsDuration() {
        foreach (TemperatureAssist assist in Assists.Where(assist => assist.CurrentDuration > TimeSpan.Zero))
            assist.CurrentDuration -= GameServer.DeltaTime;
    }

    async Task HeatDamage() {
        foreach (HeatTemperatureAssist assist in Assists.OfType<HeatTemperatureAssist>()) {
            float value = MathUtils.Map(assist.CurrentDelta, 0, assist.Limit, 0, assist.HeatDamage);

            CalculatedDamage damage = new(default, value, false, false);
            await Battle.DamageProcessor.Damage(assist.Tank, Tank, assist.WeaponMarketEntity, assist.WeaponBattleEntity, damage);
        }
    }

    void NormalizeTankTemperature() {
        if (Temperature == 0)
            return;

        TemperatureAssist[] assists = Assists.Where(assist => assist.CurrentDuration <= TimeSpan.Zero).ToArray();

        if (assists.Length == 0)
            return;

        float delta = Temperature > 0
            ? TemperatureConfig.AutoDecrementInMs
            : TemperatureConfig.AutoIncrementInMs;

        delta *= (float)TickPeriod.TotalMilliseconds;

        while (delta > 0) {
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
            TemperatureAssist? existingAssist = Assists.FirstOrDefault(a => a.Tank == assist.Tank &&
                                                                            a.WeaponMarketEntity == assist.WeaponMarketEntity);

            if (existingAssist != null && existingAssist.CanMerge(assist)) {
                Assists.Remove(existingAssist);
                assist.MergeWith(existingAssist);
            }

            NormalizeWithOpposite(assist);

            if (assist.CurrentDelta == 0)
                continue;

            Assists.Add(assist);
        }
    }

    void NormalizeAllAssists() {
        float excess = Math.Abs(Assists.Sum(assist => assist.CurrentDelta)) - 1;

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

        if (sign == 0 || assist.CurrentDelta == 0 || sign == assist.LimitSign)
            return;

        foreach (TemperatureAssist oppositeAssist in Assists.Where(a => a.LimitSign != assist.LimitSign)) {
            float ownDelta = Math.Abs(assist.CurrentDelta);
            float oppositeDelta = Math.Abs(oppositeAssist.CurrentDelta);
            float subtract = Math.Min(ownDelta, oppositeDelta);

            oppositeAssist.Subtract(subtract);
            assist.Subtract(subtract);
        }

        Assists.RemoveAll(a => a.CurrentDelta == 0);
    }

    async Task UpdateTemperatureAndSpeed() {
        Temperature = Assists.Sum(assist => assist.CurrentDelta);
        await Tank.Tank.ChangeComponent<TemperatureComponent>(component => component.Temperature = Temperature);

        if (WasFrozen) {
            await Tank.UpdateSpeed();

            if (Temperature >= 0)
                await ResetSpeed();
        }
    }

    async Task ResetSpeed() =>
        await Tank.BattlePlayer.PlayerConnection.Send(new ResetTankSpeedEvent(), Tank.Tank);
}
