using JetBrains.Annotations;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Mode;

public abstract class ModeHandler(
    Round round
) : IDisposable {
    protected Round Round { get; } = round;
    public abstract IEntity Entity { get; }

    public abstract bool HasEnemies { get; }

    public virtual Task Init() => Task.CompletedTask;

    public virtual async Task PlayerJoined(BattlePlayer player) {
        if (player is Tanker)
            await SortAllPlayers();
    }

    public virtual async Task PlayerExited(BattlePlayer player) {
        if (player is Tanker)
            await SortAllPlayers();
    }

    public virtual Task OnWarmUpEnded() => Task.CompletedTask;

    public virtual Task OnRoundEnded() => Task.CompletedTask;

    public abstract Task SortAllPlayers();

    public abstract SpawnPoint GetRandomSpawnPoint(Tanker tanker);

    public abstract int CalculateReputationDelta(Tanker tanker);

    public virtual Task Tick(TimeSpan deltaTime) => Task.CompletedTask;

    protected async Task SortPlayers(IEnumerable<Tanker> tankers) {
        var entries = tankers
            .Select(tanker => tanker.Tank.Entities.RoundUser)
            .Select(roundUser => new { User = roundUser, Stats = roundUser.GetComponent<RoundUserStatisticsComponent>()})
            .OrderByDescending(info => info.Stats.ScoreWithoutBonuses)
            .Select((info, index) => new { Info = info, NewPlace = index + 1 });

        foreach (var entry in entries.Where(entry => entry.Info.Stats.Place != entry.NewPlace)) {
            var info = entry.Info;

            info.Stats.Place = entry.NewPlace;
            await info.User.ChangeComponent(info.Stats);
            await Round.Players.Send(new SetScoreTablePositionEvent(entry.NewPlace), info.User);
        }
    }

    [Pure, LinqTunnel] // public static... meh
    public static SpawnPoint GetRandomSpawnPoint(IEnumerable<SpawnPoint> spawnPoints, params IEnumerable<SpawnPoint> exclude) =>
        spawnPoints.Except(exclude).ToList().RandomElement();

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            // todo dispose entities
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ModeHandler() => Dispose(false);
}

public delegate SpawnPoint GetRandomSpawnPoint(IEnumerable<SpawnPoint> spawnPoints, params IEnumerable<SpawnPoint> exclude);
