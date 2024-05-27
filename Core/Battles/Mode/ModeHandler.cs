using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.Server;

namespace Vint.Core.Battles.Mode;

public abstract class ModeHandler(
    Battle battle
) {
    protected Battle Battle { get; } = battle;

    public abstract Task Tick();

    public abstract SpawnPoint GetRandomSpawnPoint(BattlePlayer battlePlayer);

    protected async Task SortPlayers(IEnumerable<BattlePlayer> players) {
        foreach (var roundUserToPlace in players
                     .Where(battlePlayer => battlePlayer.InBattleAsTank)
                     .Select(battlePlayer => battlePlayer.Tank!.RoundUser)
                     .OrderByDescending(roundUser => roundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses)
                     .Select((roundUser, index) => new { RoundUser = roundUser, Place = index + 1 })
                     .Where(roundUserToPlace =>
                         roundUserToPlace.RoundUser.GetComponent<RoundUserStatisticsComponent>().Place != roundUserToPlace.Place)) {
            SetScoreTablePositionEvent @event = new(roundUserToPlace.Place);
            await roundUserToPlace.RoundUser.ChangeComponent<RoundUserStatisticsComponent>(component => component.Place = roundUserToPlace.Place);

            foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
                await battlePlayer.PlayerConnection.Send(@event, roundUserToPlace.RoundUser);
        }
    }

    public abstract Task SortPlayers();

    public abstract Task UpdateScore(IEntity? team, int score);

    public abstract Task OnWarmUpCompleted();

    public abstract Task OnFinished();

    public abstract void TransferParameters(ModeHandler previousHandler);

    public abstract BattlePlayer SetupBattlePlayer(IPlayerConnection player);

    public virtual async Task RemoveBattlePlayer(BattlePlayer player) =>
        await player.PlayerConnection.User.RemoveComponent<TeamColorComponent>();

    public abstract Task PlayerEntered(BattlePlayer player);

    public abstract Task PlayerExited(BattlePlayer player);

    public abstract int CalculateReputationDelta(BattlePlayer player);
}
