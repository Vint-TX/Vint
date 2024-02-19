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

    public override void Tick() { }

    public override void UpdateScore(IEntity? team, int score) {
        int maxScore = Battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Max(battlePlayer => battlePlayer.Tank!.RoundUser
                .GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses);

        Battle.Entity.ChangeComponent<ScoreLimitComponent>(component => component.ScoreLimit = maxScore);
    }

    public override SpawnPoint GetRandomSpawnPoint(BattlePlayer battlePlayer) {
        SpawnPoint spawnPoint = SpawnPoints
            .Shuffle()
            .First(spawnPoint => spawnPoint.Number != LastSpawnPoint?.Number &&
                                 spawnPoint.Number != battlePlayer.Tank?.SpawnPoint?.Number &&
                                 spawnPoint.Number != battlePlayer.Tank?.PreviousSpawnPoint?.Number);

        LastSpawnPoint = spawnPoint;
        return spawnPoint;
    }

    public override void SortPlayers() => SortPlayers(Battle.Players);

    public override void OnWarmUpCompleted() { }

    public override void OnFinished() { }

    public override void TransferParameters(ModeHandler previousHandler) {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => !battlePlayer.IsSpectator))
            battlePlayer.Team = null;
    }

    public override BattlePlayer SetupBattlePlayer(IPlayerConnection player) {
        BattlePlayer tankPlayer = new(player, Battle, null, false);
        return tankPlayer;
    }

    public override void PlayerEntered(BattlePlayer player) { }

    public override void PlayerExited(BattlePlayer player) => UpdateScore(null, 0);
}