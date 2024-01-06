using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public sealed class BattleStateManager : StateManager {
    public BattleStateManager(Battle battle) {
        Battle = battle;
        CurrentState = Battle.IsCustom ? new NotStarted(this) : new NotEnoughPlayers(this);
    }

    public override State CurrentState { get; protected set; }
    public Battle Battle { get; }
}