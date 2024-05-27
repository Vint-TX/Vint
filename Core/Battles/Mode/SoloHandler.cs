using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public abstract class SoloHandler(
    Battle battle
) : ModeHandler(battle) {
    protected abstract List<SpawnPoint> SpawnPoints { get; }
    protected SpawnPoint? LastSpawnPoint { get; set; }

    public override Task Tick() => Task.CompletedTask;

    public override async Task UpdateScore(IEntity? team, int score) {
        int maxScore = Battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Max(battlePlayer => battlePlayer.Tank!.RoundUser
                .GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses);

        await Battle.Entity.ChangeComponent<ScoreLimitComponent>(component => component.ScoreLimit = maxScore);
    }

    public override SpawnPoint GetRandomSpawnPoint(BattlePlayer battlePlayer) {
        SpawnPoint spawnPoint = SpawnPoints
            .Shuffle()
            .FirstOrDefault(spawnPoint => spawnPoint.Number != LastSpawnPoint?.Number &&
                                          spawnPoint.Number != battlePlayer.Tank?.SpawnPoint.Number &&
                                          spawnPoint.Number != battlePlayer.Tank?.PreviousSpawnPoint.Number,
                SpawnPoints.First());

        LastSpawnPoint = spawnPoint;
        return spawnPoint;
    }

    public override Task SortPlayers() => SortPlayers(Battle.Players);

    public override Task OnWarmUpCompleted() => Task.CompletedTask;

    public override Task OnFinished() => Task.CompletedTask;

    public override void TransferParameters(ModeHandler previousHandler) {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => !battlePlayer.IsSpectator))
            battlePlayer.Team = null;
    }

    public override BattlePlayer SetupBattlePlayer(IPlayerConnection player) {
        BattlePlayer tankPlayer = new(player, Battle, null, false);
        return tankPlayer;
    }

    public override Task PlayerEntered(BattlePlayer player) => Task.CompletedTask;

    public override async Task PlayerExited(BattlePlayer player) =>
        await UpdateScore(null, 0);
}
