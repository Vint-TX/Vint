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
    DateTimeOffset LastTickTime { get; set; }
    TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    public override async Task Start() {
        await base.Start();

        LastTickTime = StartTime = DateTimeOffset.UtcNow;

        int cooldownMs = (int)Math.Ceiling(Module.Cooldown.TotalMilliseconds);
        Module.SlotEntity.AddComponent(new InventoryCooldownStateComponent(cooldownMs, StartTime));

        if (Module.CurrentAmmo <= 0)
            Module.TryBlock(true, cooldownMs);
    }

    public override async Task Tick() {
        DateTimeOffset tickTime = DateTimeOffset.UtcNow;
        TimeSpan deltaTime = tickTime - LastTickTime;

        Elapsed += deltaTime * Module.Tank.ModuleCooldownCoeff;
        CheckForCooldownEnd();
        LastTickTime = tickTime;

        await base.Tick();
    }

    public override void Finish() {
        base.Finish();

        Module.SlotEntity.RemoveComponent<InventoryCooldownStateComponent>();
    }

    public void AddElapsedTime(TimeSpan delta) =>
        Elapsed += delta;

    void CheckForCooldownEnd() {
        if (Elapsed < Module.Cooldown) return;

        Module.SetAmmo(Module.CurrentAmmo + 1);

        if (Module.CurrentAmmo >= Module.MaxAmmo)
            StateManager.SetState(new Ready(StateManager));
        else
            StateManager.SetState(new Cooldown(StateManager));
    }
}
