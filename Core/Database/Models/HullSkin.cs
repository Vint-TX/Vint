using Microsoft.EntityFrameworkCore;

namespace Vint.Core.Database.Models;

[PrimaryKey(nameof(PlayerId), nameof(HullId), nameof(Id))]
public class HullSkin {
    HullSkin(long id, long hullId) {
        Id = id;
        HullId = hullId;
    }

    public HullSkin(Player player, long id, long hullId) : this(id, hullId) {
        Player = player;
        PlayerId = player.Id;
    }
    
    public long Id { get; private set; }

    public long HullId { get; private set; }

    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}