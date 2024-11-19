using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.Server;
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
    TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    public override async Task Start() {
        await base.Start();

        await Module.SlotEntity.AddComponent(new InventoryCooldownStateComponent(Module.Cooldown, DateTimeOffset.UtcNow));

        if (Module.CurrentAmmo <= 0)
            await Module.TryBlock();
    }

    public override async Task Tick() {
        Elapsed += GameServer.DeltaTime * Module.Tank.ModuleCooldownCoeff;
        await TryFinish();

        await base.Tick();
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
