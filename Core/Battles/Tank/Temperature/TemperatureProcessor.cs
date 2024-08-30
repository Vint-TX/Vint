using System.Collections.Concurrent;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Tank;

namespace Vint.Core.Battles.Tank.Temperature;

public class TemperatureProcessor(
    BattleTank tank
) {
    TemperatureConfigComponent TemperatureConfig { get; } =
        ConfigManager.GetComponent<TemperatureConfigComponent>(tank.Tank.TemplateAccessor!.ConfigPath!);

    ConcurrentQueue<TemperatureAssist> NewAssists { get; } = [];
    List<TemperatureAssist> Assists { get; } = [];
    public float Temperature { get; private set; } = 0;

    public async Task Tick() {
        ApplyNewAssists();
        NormalizeAllAssists();
    }

    void ApplyNewAssists() {
        while (NewAssists.TryDequeue(out TemperatureAssist? assist)) {
            TemperatureAssist? existingAssist = Assists.FirstOrDefault(a => a.Tank == assist.Tank);

            if (existingAssist != null) {
                Assists.Remove(existingAssist);
                assist = MergeTwoAssists(assist, existingAssist);
            }

            if (assist == null || assist.CurrentDelta == 0)
                continue;

            Assists.Add(assist);
        }
    }

    void NormalizeAllAssists() {
        Assists.RemoveAll(assist => assist.Sign == 0);

        if (Assists.Count == 0)
            return;

        int sign = Assists.First().Sign;
        float sum = Assists.Sum(assist => assist.CurrentDelta);

        if (Assists.TrueForAll(assist => assist.Sign == sign) &&
            (sum < 0 && sum >= TemperatureConfig.MinTemperature ||
             sum > 0 && sum <= TemperatureConfig.MaxTemperature))
            return;

        if (sum == 0) {
            Assists.Clear();
            return;
        }

        List<TemperatureAssist> heatAssists = Assists.Where(assist => assist.Sign > 0).ToList();
        List<TemperatureAssist> freezeAssists = Assists.Where(assist => assist.Sign < 0).ToList();

        float heatSum = heatAssists.Sum(assist => assist.CurrentDelta);
        float freezeSum = freezeAssists.Sum(assist => assist.CurrentDelta);

        if (heatSum > freezeSum) {
            foreach (TemperatureAssist temperatureAssist in freezeAssists) {

            }
        } else {
            // todo
        }
    }

    static TemperatureAssist? MergeTwoAssists(TemperatureAssist first, TemperatureAssist second) {
        float sum = first.CurrentDelta + second.CurrentDelta;
        int sign;
        TemperatureAssist result;

        if (first.Sign == second.Sign) {
            result = Math.Abs(first.Limit) > Math.Abs(second.Limit) ? first : second;
            sign = result.Sign;
        } else {
            sign = Math.Sign(sum);

            if (sign == 0)
                return null;

            result = sign == first.Sign ? first : second;
        }

        result.CurrentDelta = sign * Math.Min(Math.Abs(sum), Math.Abs(result.Limit));
        return result;
    }

    public void EnqueueAssist(TemperatureAssist assist) =>
        NewAssists.Enqueue(assist);

    public void ChangeTemperatureConfig(float incrementTemperatureDelta, float decrementTemperatureDelta) {
        TemperatureConfig.AutoIncrementInMs += incrementTemperatureDelta;
        TemperatureConfig.AutoDecrementInMs += decrementTemperatureDelta;
    }
}
