using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.Server;

namespace Vint.Core.Battles.Mode;

public abstract class ModeHandler(
    Battle battle
) {
    public Battle Battle { get; } = battle;

    public abstract BattleMode BattleMode { get; }

    public abstract void Tick();

    public abstract SpawnPoint GetRandomSpawnPoint();

    public virtual void SortScoreTable() {
        foreach (var roundUserToPlace in Battle.Players
                     .ToArray()
                     .Where(battlePlayer => battlePlayer.InBattleAsTank)
                     .Select(battlePlayer => battlePlayer.Tank!.RoundUser)
                     .OrderBy(roundUser => roundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses)
                     .Select((roundUser, index) => new { RoundUser = roundUser, Place = index + 1 })
                     .Where(roundUserToPlace =>
                         roundUserToPlace.RoundUser.GetComponent<RoundUserStatisticsComponent>().Place != roundUserToPlace.Place)) {
            SetScoreTablePositionEvent @event = new(roundUserToPlace.Place);
            roundUserToPlace.RoundUser.ChangeComponent<RoundUserStatisticsComponent>(component => component.Place = roundUserToPlace.Place);

            foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
                battlePlayer.PlayerConnection.Send(@event, roundUserToPlace.RoundUser);
        }
    }

    public abstract void OnStarted();

    public abstract void OnFinished();

    public abstract void TransferParameters(ModeHandler previousHandler);

    public abstract BattlePlayer SetupBattlePlayer(IPlayerConnection player);

    public abstract void RemoveBattlePlayer(BattlePlayer player);

    public abstract void PlayerEntered(BattlePlayer player);

    public abstract void PlayerExited(BattlePlayer player);
}