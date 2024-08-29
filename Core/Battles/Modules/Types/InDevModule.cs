using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class InDevModule : BattleModule {
    public override string ConfigPath => "";

    public override Effect GetEffect() => throw new NotSupportedException();

    public override async Task Activate() {
        IPlayerConnection connection = Tank.BattlePlayer.PlayerConnection;

        await ChatUtils.SendMessage("This module is not implemented yet", ChatUtils.GetChat(connection), [connection], null);
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        Tank = tank;
        SlotUserEntity = userSlot;
        MarketEntity = marketModule;
        UserEntity = marketModule.GetUserModule(tank.BattlePlayer.PlayerConnection);

        Level = (int)UserEntity.GetComponent<ModuleUpgradeLevelComponent>().Level;
        SlotEntity = await CreateBattleSlot();
        Entity = await CreateBattleModule();
    }

    protected override async Task<IEntity> CreateBattleSlot() {
        IEntity clone = SlotUserEntity.Clone();
        clone.Id = EntityRegistry.GenerateId();

        await clone.AddGroupComponent<TankGroupComponent>(Tank.Tank);
        await clone.AddComponent(new InventorySlotTemporaryBlockedByServerComponent());
        await clone.AddComponent(new InventoryAmmunitionComponent(1));
        return clone;
    }

    public override Task TryBlock() => Task.CompletedTask;

    public override Task TryUnblock() => Task.CompletedTask;
}
