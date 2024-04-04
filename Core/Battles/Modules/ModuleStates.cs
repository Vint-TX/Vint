using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Modules;

public class ModuleState(
    ModuleStateManager stateManager
) : State {
    public override ModuleStateManager StateManager { get; } = stateManager;
    public BattleModule Module => StateManager.Module;
}

public class Ready(
    ModuleStateManager stateManager
) : ModuleState(stateManager);

public class Cooldown(
    ModuleStateManager stateManager
) : ModuleState(stateManager) {
    public bool CanBeUsed => Module.CurrentAmmo > 0;
    DateTimeOffset StartTime { get; set; }
    DateTimeOffset EndTime { get; set; }

    public override void Start() {
        base.Start();

        int cooldownMs = (int)Math.Ceiling(Module.Cooldown.TotalMilliseconds);

        StartTime = DateTimeOffset.UtcNow;
        EndTime = StartTime + Module.Cooldown;
        Module.SlotEntity.AddComponent(new InventoryCooldownStateComponent(cooldownMs, StartTime));

        if (Module.CurrentAmmo <= 0)
            Module.TryBlock(true, cooldownMs);
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow < EndTime) return;

        Module.SetAmmo(Module.CurrentAmmo + 1);

        if (Module.CurrentAmmo >= Module.MaxAmmo)
            StateManager.SetState(new Ready(StateManager));
        else {
            StartTime = DateTimeOffset.UtcNow;
            EndTime = StartTime + Module.Cooldown;
            Module.SlotEntity.RemoveComponent<InventoryCooldownStateComponent>();
            Module.SlotEntity.AddComponent(new InventoryCooldownStateComponent((int)Math.Ceiling(Module.Cooldown.TotalMilliseconds), StartTime));
        }
    }

    public override void Finish() {
        base.Finish();

        Module.SlotEntity.RemoveComponent<InventoryCooldownStateComponent>();
    }

    public void UpdateDuration() => EndTime = StartTime + Module.Cooldown;
}