using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Modules;

public class ModuleStateManager : StateManager<ModuleState> {
    public ModuleStateManager(BattleModule module) {
        Module = module;
        CurrentState = new Ready(this);
    }

    public BattleModule Module { get; }
}