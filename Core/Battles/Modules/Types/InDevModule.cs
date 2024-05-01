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

    public override void Activate() {
        IPlayerConnection connection = Tank.BattlePlayer.PlayerConnection;

        Task.Run(async () => await ChatUtils.SendMessage("This module is not implemented yet", ChatUtils.GetChat(connection), [connection], null));
    }

    public override void Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        IEntity userModule = marketModule.GetUserModule(tank.BattlePlayer.PlayerConnection);
        MarketEntity = marketModule;
        Tank = tank;

        Level = (int)userModule.GetComponent<ModuleUpgradeLevelComponent>().Level;
        SlotEntity = CreateBattleSlot(Tank, userSlot);
        Entity = new ModuleUserItemTemplate().Create(Tank, userModule);
    }

    protected override IEntity CreateBattleSlot(BattleTank tank, IEntity userSlot) {
        IEntity clone = userSlot.Clone();
        clone.Id = EntityRegistry.FreeId;

        clone.AddGroupComponent<TankGroupComponent>(Tank.Tank);
        clone.AddComponent(new InventorySlotTemporaryBlockedByServerComponent(9999999, DateTimeOffset.UtcNow));
        clone.AddComponent(new InventoryAmmunitionComponent(1));
        return clone;
    }

    public override void TryBlock(bool force = false, long blockTimeMs = 0) { }

    public override void TryUnblock() { }
}
