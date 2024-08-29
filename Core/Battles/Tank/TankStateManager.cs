using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Tank;

public sealed class TankStateManager : StateManager<TankState> {
    public TankStateManager(BattleTank battleTank) {
        BattleTank = battleTank;
        CurrentState = new New(this);
    }

    public BattleTank BattleTank { get; }
}
