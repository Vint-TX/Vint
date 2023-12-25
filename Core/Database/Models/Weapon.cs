using Microsoft.EntityFrameworkCore;

namespace Vint.Core.Database.Models;

[PrimaryKey(nameof(PlayerId), nameof(Id))]
public class Weapon {
    Weapon(long id, long skinId, long shellId) {
        Id = id;
        SkinId = skinId;
        ShellId = shellId;
    }

    public Weapon(Player player, long id, long skinId, long shellId) : this(id, skinId, shellId) {
        Player = player;
        PlayerId = player.Id;
    }

    public long Id { get; private set; }

    public long Xp { get; set; }
    
    public long SkinId { get; set; }
    public long ShellId { get; set; }
    
    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}