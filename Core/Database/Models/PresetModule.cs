using LinqToDB.Mapping;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.Server.Game;

namespace Vint.Core.Database.Models;

[Table(DbConstants.PresetModules)]
public class PresetModule {
    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))] [field: NotColumn]
    public required Player Player {
        get;
        init {
            field = value;
            PlayerId = value.Id;
        }
    } = null!;

    [PrimaryKey(0)] public long PlayerId { get; private set; }
    [PrimaryKey(1)] public int PresetIndex { get; private set; }
    [PrimaryKey(2)] public required Slot Slot { get; init; }

    [Column] public IEntity Entity { get; set; } = null!;

    [Association(ThisKey = $"{nameof(PlayerId)},{nameof(PresetIndex)}", OtherKey = $"{nameof(Preset.PlayerId)},{nameof(Preset.Index)}")]
    [field: NotColumn]
    public required Preset Preset {
        get;
        init {
            field = value;
            PresetIndex = value.Index;
        }
    } = null!;

    public IEntity GetSlotEntity(IPlayerConnection connection) =>
        connection.SharedEntities.Single(entity => entity.TemplateAccessor?.Template is SlotUserItemTemplate &&
                                                   entity.GetComponent<SlotUserItemInfoComponent>()
                                                       .Slot ==
                                                   Slot);
}
