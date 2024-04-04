using LinqToDB.Mapping;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.Server;

namespace Vint.Core.Database.Models;

[Table("PresetModules")]
public class PresetModule {
    [NotColumn] Player _player = null!;
    [NotColumn] Preset _preset = null!;

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public Player Player {
        get => _player;
        set {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
    [PrimaryKey(1)] public int PresetIndex { get; private set; }
    [PrimaryKey(2)] public Slot Slot { get; init; }

    [Column] public IEntity Entity { get; set; } = null!;

    [Association(ThisKey = $"{nameof(PlayerId)},{nameof(PresetIndex)}", OtherKey = $"{nameof(Preset.PlayerId)},{nameof(Preset.Index)}")]
    public Preset Preset {
        get => _preset;
        set {
            _preset = value;
            PresetIndex = value.Index;
        }
    }

    public IEntity GetSlotEntity(IPlayerConnection connection) =>
        connection.SharedEntities.Single(entity => entity.TemplateAccessor?.Template is SlotUserItemTemplate &&
                                                   entity.GetComponent<SlotUserItemInfoComponent>().Slot == Slot);
}