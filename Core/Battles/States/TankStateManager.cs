using Vint.Core.Battles.Player;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public sealed class TankStateManager : StateManager {
    public TankStateManager(BattleTank battleTank) {
        BattleTank = battleTank;
        CurrentState = new New(this);
    }

    public override State CurrentState { get; protected set; }
    public BattleTank BattleTank { get; }
}