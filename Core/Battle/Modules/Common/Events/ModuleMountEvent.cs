using LinqToDB;
using Vint.Core.Battle.Modules.Common.Components;
using Vint.Core.Battle.Modules.Common.Components.Slot;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Events;

[ProtocolId(1485777098598)]
public class ModuleMountEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity moduleUserItem = entities[0];
        IEntity slotUserItem = entities[1];

        if (moduleUserItem.HasComponent<MountedItemComponent>() ||
            slotUserItem.HasComponent<ModuleGroupComponent>()) return;

        Database.Models.Player player = connection.Player;

        long marketItemId = moduleUserItem.GetComponent<MarketItemGroupComponent>().Key;
        Module? module = player.Modules.FirstOrDefault(module => module.Id == marketItemId);

        if (module == null || module.Level < 0)
            return;

        Slot slot = slotUserItem.GetComponent<SlotUserItemInfoComponent>().Slot;

        await using (DbConnection db = new()) {
            PresetModule? presetModule = await db.PresetModules
                .FirstOrDefaultAsync(pModule => pModule.PlayerId == player.Id &&
                                                pModule.PresetIndex == player.CurrentPresetIndex &&
                                                pModule.Slot == slot);

            presetModule ??= new PresetModule { Player = player, Preset = player.CurrentPreset, Slot = slot };
            presetModule.Entity = connection.GetEntity(marketItemId)!;

            await db.InsertOrReplaceAsync(presetModule);

            player.CurrentPreset.Modules.RemoveAll(pModule => pModule.Slot == slot);
            player.CurrentPreset.Modules.Add(presetModule);
        }

        await slotUserItem.AddComponentFrom<ModuleGroupComponent>(moduleUserItem);
        await moduleUserItem.AddComponent<MountedItemComponent>();
    }
}
