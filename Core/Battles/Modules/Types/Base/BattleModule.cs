using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Module;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types.Base;

public abstract class BattleModule {
    protected BattleModule() =>
        StateManager = new ModuleStateManager(this);

    public abstract string ConfigPath { get; }
    public int Level { get; protected set; }

    public ModuleStateManager StateManager { get; }
    public BattleTank Tank { get; protected set; } = null!;
    public Battle Battle => Tank.Battle;
    public IEntity MarketEntity { get; protected set; } = null!;

    public IEntity Entity { get; protected set; } = null!;
    public IEntity SlotEntity { get; protected set; } = null!;

    protected IEntity UserEntity { get; set; } = null!;
    protected IEntity SlotUserEntity { get; set; } = null!;

    public int CurrentAmmo { get; private set; }
    public int MaxAmmo { get; private set; }
    public TimeSpan Cooldown { get; private set; }

    TimeSpan EMPLockTime { get; set; }
    bool IsEMPLocked { get; set; }
    bool IsBlocked => SlotEntity.HasComponent<InventorySlotTemporaryBlockedByServerComponent>();
    bool IsEnabled => SlotEntity.HasComponent<InventoryEnabledStateComponent>();
    protected virtual bool ActivationCondition => true;
    protected bool CanBeActivated => !IsBlocked &&
                                     !IsEMPLocked &&
                                     IsEnabled &&
                                     ActivationCondition &&
                                     StateManager.CurrentState is Ready or Cooldown { CanBeUsed: true } &&
                                     Tank.StateManager.CurrentState is Active &&
                                     Battle.StateManager.CurrentState is Running or WarmUp { WarmUpStateManager.CurrentState: WarmingUp };

    public abstract Effect GetEffect();

    public virtual async Task Activate() =>
        await SetAmmo(CurrentAmmo - 1);

    public virtual async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        Tank = tank;
        SlotUserEntity = userSlot;
        MarketEntity = marketModule;
        UserEntity = marketModule.GetUserModule(tank.BattlePlayer.PlayerConnection);

        Level = (int)UserEntity.GetComponent<ModuleUpgradeLevelComponent>().Level;
        CurrentAmmo = MaxAmmo = (int)GetStat<ModuleAmmunitionPropertyComponent>();
        Cooldown = TimeSpan.FromMilliseconds(GetStat<ModuleCooldownPropertyComponent>());

        SlotEntity = await CreateBattleSlot();
        Entity = await CreateBattleModule();
    }

    public async Task SwitchToBattleEntities() {
        IPlayerConnection connection = Tank.BattlePlayer.PlayerConnection;

        await connection.Unshare(SlotUserEntity, UserEntity);
        await connection.Share(SlotEntity, Entity);
    }

    public async Task SwitchToUserEntities() {
        IPlayerConnection connection = Tank.BattlePlayer.PlayerConnection;

        await connection.Unshare(SlotEntity, Entity);
        await connection.Share(SlotUserEntity, UserEntity);
    }

    protected virtual async Task<IEntity> CreateBattleSlot() {
        IEntity clone = SlotUserEntity.Clone();
        clone.Id = EntityRegistry.GenerateId();

        await clone.AddGroupComponent<TankGroupComponent>(Tank.Tank);
        await clone.AddComponent<InventoryEnabledStateComponent>();
        await clone.AddComponent(new InventoryAmmunitionComponent(MaxAmmo));
        return clone;
    }

    protected virtual async Task<IEntity> CreateBattleModule() {
        IEntity clone = UserEntity.Clone();
        clone.Id = EntityRegistry.GenerateId();

        await clone.AddComponent<ModuleUsesCounterComponent>();
        await clone.AddGroupComponent<TankGroupComponent>(Tank.Tank);
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

    public virtual async Task Tick() {
        await StateManager.Tick();

        if (IsEMPLocked) {
            EMPLockTime -= GameServer.DeltaTime;
            await TryEMPUnlock();
        }
    }

    public virtual Task TryBlock() =>
        SlotEntity.AddComponentIfAbsent(new InventorySlotTemporaryBlockedByServerComponent());

    public virtual Task TryUnblock() =>
        CurrentAmmo <= 0
            ? Task.CompletedTask
            : SlotEntity.RemoveComponentIfPresent<InventorySlotTemporaryBlockedByServerComponent>();

    public async Task EMPLock(TimeSpan duration) { // effects should be deactivated separately
        EMPLockTime += duration;

        if (IsEMPLocked)
            return;

        IsEMPLocked = true;
        await SlotEntity.AddComponentIfAbsent<SlotLockedByEMPComponent>();
        await TryBlock();
    }

    async Task TryEMPUnlock() {
        if (!IsEMPLocked || EMPLockTime > TimeSpan.Zero)
            return;

        await TryUnblock();
        await SlotEntity.RemoveComponentIfPresent<SlotLockedByEMPComponent>();

        IsEMPLocked = false;
        EMPLockTime = TimeSpan.Zero;

        if (this is IAlwaysActiveModule)
            await Activate();
    }

    protected float GetStat<T>() where T : ModuleEffectUpgradablePropertyComponent =>
        Leveling.GetStat<T>(ConfigPath, Level);
}
