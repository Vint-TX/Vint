using System.ComponentModel.DataAnnotations;
using Vint.Core.ECS;

namespace Vint.Core.Database.Models;

public class Cover {
    Cover(long id) => Id = id;

    public Cover(Player player, long id) : this(id) {
        Player = player;
        PlayerId = player.Id;
    }

    [Key] public long Id { get; private set; }

    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}
