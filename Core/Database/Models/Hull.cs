using Microsoft.EntityFrameworkCore;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Database.Models;

[PrimaryKey(nameof(PlayerId), nameof(Id))]
public class Hull {
    Hull(long id, long skinId) {
        Id = id;
        SkinId = skinId;
    }

    public Hull(Player player, long id, long skinId) : this(id, skinId) {
        Player = player;
        PlayerId = player.Id;
    }

    public long Id { get; private set; }

    public long Xp { get; set; }
    
    public long SkinId { get; set; }
    
    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}