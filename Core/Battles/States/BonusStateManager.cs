using Vint.Core.Battles.Bonus;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public class BonusStateManager : StateManager<BonusState> {
    public BonusStateManager(BonusBox bonus) {
        Bonus = bonus;
        CurrentState = new None(this);
    }

    public BonusBox Bonus { get; }
}