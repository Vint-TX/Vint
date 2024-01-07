using LinqToDB.Mapping;
using Vint.Core.ECS.Enums;

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

    [Column] public long Id { get; set; }

    [Association(ThisKey = $"{nameof(PlayerId)},{nameof(PresetIndex)}", OtherKey = $"{nameof(Preset.PlayerId)},{nameof(Preset.Index)}")]
    public Preset Preset {
        get => _preset;
        set {
            _preset = value;
            PresetIndex = value.Index;
        }
    }
}