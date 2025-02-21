using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.StateMachine;

namespace Vint.Core.Battle.Modules;

public class ModuleStateManager(
    BattleModule module
) : StateManager<ModuleState> {
    public BattleModule Module { get; } = module;

    public override Task Init() =>
        InitState(new Ready(this));
}


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
    TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    public override async Task Start() {
        await base.Start();

        int cooldownMs = (int)Math.Ceiling(Module.Cooldown.TotalMilliseconds);
        await Module.SlotEntity.AddComponent(new InventoryCooldownStateComponent(cooldownMs, DateTimeOffset.UtcNow));

        if (Module.CurrentAmmo <= 0)
            await Module.TryBlock();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        Elapsed += deltaTime * Module.Tank.ModuleCooldownCoeff;
        await TryFinish();

        await base.Tick(deltaTime);
    }

    public override async Task Finish() {
        await base.Finish();

        await Module.SlotEntity.RemoveComponent<InventoryCooldownStateComponent>();
    }

    public void AddElapsedTime(TimeSpan delta) =>
        Elapsed += delta;

    async Task TryFinish() {
        if (Elapsed < Module.Cooldown) return;

        await Module.SetAmmo(Module.CurrentAmmo + 1);

        if (Module.CurrentAmmo >= Module.MaxAmmo)
            await StateManager.SetState(new Ready(StateManager));
        else
            await StateManager.SetState(new Cooldown(StateManager));
    }
}
