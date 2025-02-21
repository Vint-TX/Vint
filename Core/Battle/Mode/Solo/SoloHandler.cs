using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Mode.Solo;

public abstract class SoloHandler(
    Round round
) : ModeHandler(round) {
    protected abstract IList<SpawnPoint> SpawnPoints { get; }
    SpawnPoint LastSpawnPoint { get; set; }

    public override bool HasEnemies => Round.Tankers.Count() >= 2;

    public async Task TryUpdateScore() {
        List<Tanker> tankers = Round.Tankers.ToList();

        if (tankers.Count == 0)
            return;

        int maxScore = tankers.Max(tanker => tanker.Tank.Entities.RoundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses);
        ScoreLimitComponent scoreLimitComponent = Entity.GetComponent<ScoreLimitComponent>();

        if (scoreLimitComponent.ScoreLimit != maxScore) {
            scoreLimitComponent.ScoreLimit = maxScore;
            await Entity.ChangeComponent(scoreLimitComponent);
            await Round.Players.Send(new RoundScoreUpdatedEvent(), Round.Entity);
        }
    }

    public override SpawnPoint GetRandomSpawnPoint(Tanker tanker) {
        BattleTank tank = tanker.Tank;
        LastSpawnPoint = GetRandomSpawnPoint(SpawnPoints, LastSpawnPoint, tank.SpawnPoint, tank.PreviousSpawnPoint);
        return LastSpawnPoint;
    }

    public override async Task SortAllPlayers() => await SortPlayers(Round.Tankers);

    public override async Task PlayerExited(BattlePlayer player) {
        await base.PlayerExited(player);

        if (player is Tanker)
            await TryUpdateScore();
    }
}
