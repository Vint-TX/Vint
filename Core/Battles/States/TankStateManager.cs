using Vint.Core.Battles.Player;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public sealed class TankStateManager : StateManager<TankState> {
    public TankStateManager(BattleTank battleTank) {
        BattleTank = battleTank;
        CurrentState = new New(this);
    }

    public BattleTank BattleTank { get; }
}