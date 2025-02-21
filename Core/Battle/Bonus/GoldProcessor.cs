using Serilog;
using Vint.Core.Battle.Bonus.Type;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Enums;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Bonus;

public class GoldProcessor(
    Round round,
    GoldBox[] goldBoxes,
    GoldMapInfo goldInfo
) {
    public const int MaxGoldsPerRound = 16;
    public int GoldsDropped { get; private set; }

    ILogger Logger { get; } = Log.Logger.ForType<GoldProcessor>();

    bool CanRandomDrop => round.Properties.Type == BattleType.Rating &&
                          round.StateManager.CurrentState is Running;

    double LastAchievedScoreThreshold { get; set; }
    double NextScoreThreshold { get; set; }
    int CurrentScore { get; set; }
    int PlayersCount { get; set; }

    public async Task ScoreChanged(int delta) {
        if (!CanRandomDrop || delta <= 0)
            return;

        CurrentScore += delta;

        if (CurrentScore >= NextScoreThreshold) {
            Logger.Information("Gold drop triggered. Current score: {CurrentScore}, threshold: {NextScoreThreshold}", CurrentScore, NextScoreThreshold);

            LastAchievedScoreThreshold = NextScoreThreshold;
            CalculateNextThreshold();
            await TryDrop();
        }
    }

    public void PlayersCountChanged(int currentCount) {
        if (!CanRandomDrop) return;

        PlayersCount = currentCount;
        CalculateNextThreshold();
    }

    public async Task<bool> DropRandom(Tanker? tanker, bool force = false) {
        foreach (GoldBox gold in goldBoxes.Shuffle()) {
            if (await Drop(gold, tanker, force))
                return true;
        }

        return false;
    }

    void CalculateNextThreshold() {
        double delta = Math.Log(PlayersCount) * goldInfo.BaseThresholdDelta;
        NextScoreThreshold = delta * (LastAchievedScoreThreshold / delta + 1);

        Logger.Information("CalculateNextThreshold, delta: {Delta}", delta);
        Logger.Information("Next gold drop threshold: {NextScoreThreshold}", NextScoreThreshold);
    }

    async Task TryDrop() {
        if (!MathUtils.RollTheDice(goldInfo.DropProbability)) {
            Logger.Information("Gold drop skipped due to low probability");
            return;
        }

        Logger.Information("Gold dropped");
        await DropRandom(null);
    }

    async Task<bool> Drop(GoldBox gold, Tanker? tanker, bool force) {
        if (!force && GoldsDropped >= MaxGoldsPerRound)
            return false;

        if (!gold.CanBeDropped(true))
            return false;

        if (!force)
            GoldsDropped++;

        await gold.Drop(tanker);
        return true;
    }
}
