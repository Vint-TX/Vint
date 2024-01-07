using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public sealed class BattleStateManager : StateManager<BattleState> {
    public BattleStateManager(Battle battle) {
        Battle = battle;
        CurrentState = Battle.IsCustom ? new NotStarted(this) : new NotEnoughPlayers(this);
    }

    public Battle Battle { get; }
}