using LinqToDB.Mapping;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.Server;

namespace Vint.Core.Database.Models;

[Table("PresetModules")]
public class PresetModule {
    [NotColumn] readonly Player _player = null!;
    [NotColumn] readonly Preset _preset = null!;

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public required Player Player {
        get => _player;
        init {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
    [PrimaryKey(1)] public int PresetIndex { get; private set; }
    [PrimaryKey(2)] public required Slot Slot { get; init; }

    [Column] public IEntity Entity { get; set; } = null!;

    [Association(ThisKey = $"{nameof(PlayerId)},{nameof(PresetIndex)}", OtherKey = $"{nameof(Preset.PlayerId)},{nameof(Preset.Index)}")]
    public required Preset Preset {
        get => _preset;
        init {
            _preset = value;
            PresetIndex = value.Index;
        }
    }

    public IEntity GetSlotEntity(IPlayerConnection connection) =>
        connection.SharedEntities.Single(entity => entity.TemplateAccessor?.Template is SlotUserItemTemplate &&
                                                   entity.GetComponent<SlotUserItemInfoComponent>().Slot == Slot);
}
