using Microsoft.EntityFrameworkCore;

namespace Vint.Core.Database.Models;

[PrimaryKey(nameof(PlayerId), nameof(WeaponId), nameof(Id))]
public class Shell {
    Shell(long id, long weaponId) {
        Id = id;
        WeaponId = weaponId;
    }

    public Shell(Player player, long id, long weaponId) : this(id, weaponId) {
        Player = player;
        PlayerId = player.Id;
    }
    
    public long Id { get; private set; }

    public long WeaponId { get; private set; }

    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}