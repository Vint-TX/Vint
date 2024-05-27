using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Modules;
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
        IEntity userModule = marketModule.GetUserModule(tank.BattlePlayer.PlayerConnection);
        MarketEntity = marketModule;
        Tank = tank;

        Level = (int)userModule.GetComponent<ModuleUpgradeLevelComponent>().Level;
        SlotEntity = await CreateBattleSlot(Tank, userSlot);
        Entity = new ModuleUserItemTemplate().Create(Tank, userModule);
    }

    protected override async Task<IEntity> CreateBattleSlot(BattleTank tank, IEntity userSlot) {
        IEntity clone = userSlot.Clone();
        clone.Id = EntityRegistry.FreeId;

        await clone.AddGroupComponent<TankGroupComponent>(Tank.Tank);
        await clone.AddComponent(new InventorySlotTemporaryBlockedByServerComponent(9999999, DateTimeOffset.UtcNow));
        await clone.AddComponent(new InventoryAmmunitionComponent(1));
        return clone;
    }

    public override Task TryBlock(bool force = false, long blockTimeMs = 0) => Task.CompletedTask;

    public override Task TryUnblock() => Task.CompletedTask;
}
