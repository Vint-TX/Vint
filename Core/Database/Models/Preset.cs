using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Vint.Core.ECS;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Database.Models;

[PrimaryKey(nameof(PlayerId), nameof(Index))]
public class Preset {
    Preset(int index) {
        Index = index;
        Name = $"Preset {index + 1}";
    }

    public Preset(Player player, int index) : this(index) {
        Player = player;
        PlayerId = player.Id;
    }

    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }

    public int Index { get; private set; }
    public string Name { get; set; }

    public IEntity? Entity { get; set; }

    public IEntity Weapon { get; set; } = GlobalEntities.GetEntity("weapons", "Smoky");
    public IEntity Hull { get; set; } = GlobalEntities.GetEntity("hulls", "Hunter");

    public IEntity WeaponSkin { get; set; } = GlobalEntities.GetEntity("weaponSkins", "SmokyM0");
    public IEntity HullSkin { get; set; } = GlobalEntities.GetEntity("hullSkins", "HunterM0");

    public IEntity Cover { get; set; } = GlobalEntities.GetEntity("covers", "None");
    public IEntity Paint { get; set; } = GlobalEntities.GetEntity("paints", "Green");

    public IEntity Shell { get; set; } = GlobalEntities.GetEntity("shells", "SmokyStandard");
    public IEntity Graffiti { get; set; } = GlobalEntities.GetEntity("graffities", "Logo");
    // todo modules
}
