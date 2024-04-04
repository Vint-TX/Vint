using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Bonus;

public class BonusStateManager : StateManager<BonusState> {
    public BonusStateManager(BonusBox bonus) {
        Bonus = bonus;
        CurrentState = new None(this);
    }

    public BonusBox Bonus { get; }
}