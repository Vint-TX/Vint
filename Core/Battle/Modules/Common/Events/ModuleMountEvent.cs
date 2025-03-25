using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Items.Module;

[ProtocolId(1485777098598)]
public class ModuleMountEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity moduleUserItem = entities[0];
        IEntity slotUserItem = entities[1];

        if (moduleUserItem.HasComponent<MountedItemComponent>() ||
            slotUserItem.HasComponent<ModuleGroupComponent>()) return;

        Player player = connection.Player;

        long marketItemId = moduleUserItem.GetComponent<MarketItemGroupComponent>().Key;
        Database.Models.Module? module = player.Modules.SingleOrDefault(module => module.Id == marketItemId);

        if (module == null || module.Level < 0)
            return;

        Slot slot = slotUserItem.GetComponent<SlotUserItemInfoComponent>().Slot;
        await using DbConnection db = new();

        PresetModule? presetModule = await db.PresetModules.SingleOrDefaultAsync(pModule =>
            pModule.PlayerId == player.Id && pModule.PresetIndex == player.CurrentPresetIndex && pModule.Slot == slot);

        presetModule ??= new PresetModule { Player = player, Preset = player.CurrentPreset, Slot = slot };
        presetModule.Entity = connection.GetEntity(marketItemId)!;

        await db.InsertOrReplaceAsync(presetModule);

        player.CurrentPreset.Modules.RemoveAll(pModule => pModule.Slot == slot);
        player.CurrentPreset.Modules.Add(presetModule);

        await slotUserItem.AddComponentFrom<ModuleGroupComponent>(moduleUserItem);
        await moduleUserItem.AddComponent<MountedItemComponent>();
    }
}
