using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Vint.Core.ECS;

namespace Vint.Core.Database.Models;

[PrimaryKey(nameof(PlayerId), nameof(Id))]
public class Avatar {
    Avatar(long id) => Id = id;

    public Avatar(Player player, long id) : this(id) {
        Player = player;
        PlayerId = player.Id;
    }

    [Key] public long Id { get; private set; }

    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}
