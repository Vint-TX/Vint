using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Module;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types.Base;

public abstract class BattleModule {
    protected BattleModule() =>
        StateManager = new ModuleStateManager(this);

    public abstract string ConfigPath { get; }
    public int Level { get; protected set; }

    public ModuleStateManager StateManager { get; }
    public BattleTank Tank { get; protected set; } = null!;
    public IEntity MarketEntity { get; protected set; } = null!;
    public IEntity Entity { get; protected set; } = null!;
    public IEntity SlotEntity { get; protected set; } = null!;

    public int CurrentAmmo { get; private set; }
    public int MaxAmmo { get; private set; }
    public TimeSpan Cooldown { get; private set; }
    TimeSpan OriginalCooldown { get; set; }

    public virtual bool ActivationCondition => true;
    public bool IsBlocked => SlotEntity.HasComponent<InventorySlotTemporaryBlockedByServerComponent>();
    public bool IsEnabled => SlotEntity.HasComponent<InventoryEnabledStateComponent>();
    public bool CanBeActivated => !IsBlocked &&
                                  IsEnabled &&
                                  ActivationCondition &&
                                  StateManager.CurrentState is Ready or Cooldown { CanBeUsed: true } &&
                                  Tank.StateManager.CurrentState is Active &&
                                  Tank.Battle.StateManager.CurrentState is Running or WarmUp { WarmUpStateManager.CurrentState: WarmingUp };

    public abstract Effect GetEffect();

    public virtual async Task Activate() {
        await SetAmmo(CurrentAmmo - 1);

        if (StateManager.CurrentState is not Modules.Cooldown)
            await StateManager.SetState(new Cooldown(StateManager));
    }

    public virtual async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        IEntity userModule = marketModule.GetUserModule(tank.BattlePlayer.PlayerConnection);
        MarketEntity = marketModule;
        Tank = tank;

        Level = (int)userModule.GetComponent<ModuleUpgradeLevelComponent>().Level;
        CurrentAmmo = MaxAmmo = (int)Leveling.GetStat<ModuleAmmunitionPropertyComponent>(ConfigPath, Level);
        OriginalCooldown = Cooldown = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleCooldownPropertyComponent>(ConfigPath, Level));
        SlotEntity = await CreateBattleSlot(Tank, userSlot);
        Entity = new ModuleUserItemTemplate().Create(Tank, userModule);
    }

    protected virtual async Task<IEntity> CreateBattleSlot(BattleTank tank, IEntity userSlot) {
        IEntity clone = userSlot.Clone();
        clone.Id = EntityRegistry.FreeId;

        await clone.AddGroupComponent<TankGroupComponent>(tank.Tank);
        await clone.AddComponent<InventoryEnabledStateComponent>();
        await clone.AddComponent(new InventoryAmmunitionComponent(MaxAmmo));
        await clone.RemoveComponent<SlotTankPartComponent>();
        await clone.RemoveComponent<MarketItemGroupComponent>();
        return clone;
    }

    public async Task SetAmmo(int value) {
        if (value < 0 || value > MaxAmmo) return;

        CurrentAmmo = value;
        await SlotEntity.ChangeComponent<InventoryAmmunitionComponent>(component => component.CurrentCount = CurrentAmmo);
        await Tank.BattlePlayer.PlayerConnection.Send(new InventoryAmmunitionChangedEvent(), SlotEntity);

        await TryUnblock();

        if (CurrentAmmo < MaxAmmo && StateManager.CurrentState is not Modules.Cooldown)
            await StateManager.SetState(new Cooldown(StateManager));
        else if (CurrentAmmo >= MaxAmmo && StateManager.CurrentState is not Ready)
            await StateManager.SetState(new Ready(StateManager));
    }

    public Task Tick() => StateManager.Tick();

    public virtual Task TryBlock(bool force = false, long blockTimeMs = 0) =>
        SlotEntity.AddComponentIfAbsent(new InventorySlotTemporaryBlockedByServerComponent(blockTimeMs, DateTimeOffset.UtcNow));

    public virtual Task TryUnblock() =>
        CurrentAmmo <= 0
            ? Task.CompletedTask
            : SlotEntity.RemoveComponentIfPresent<InventorySlotTemporaryBlockedByServerComponent>();
}
