using LinqToDB.Mapping;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Database.Models;

[Table("Presets")]
public class Preset {
    [NotColumn] Player _player = null!;

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public Player Player {
        get => _player;
        set {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }

    [PrimaryKey(1)] public int Index { get; init; }
    [Column] public string Name { get; set; } = null!;

    [NotColumn] public IEntity? Entity { get; set; }

    [Column] public IEntity Weapon { get; set; } = GlobalEntities.GetEntity("weapons", "Smoky");
    [Column] public IEntity Hull { get; set; } = GlobalEntities.GetEntity("hulls", "Hunter");

    [Column] public IEntity WeaponSkin { get; set; } = GlobalEntities.GetEntity("weaponSkins", "SmokyM0");
    [Column] public IEntity HullSkin { get; set; } = GlobalEntities.GetEntity("hullSkins", "HunterM0");

    [Column] public IEntity Cover { get; set; } = GlobalEntities.GetEntity("covers", "None");
    [Column] public IEntity Paint { get; set; } = GlobalEntities.GetEntity("paints", "Green");

    [Column] public IEntity Shell { get; set; } = GlobalEntities.GetEntity("shells", "SmokyStandard");
    [Column] public IEntity Graffiti { get; set; } = GlobalEntities.GetEntity("graffities", "Logo");
    // todo modules
}