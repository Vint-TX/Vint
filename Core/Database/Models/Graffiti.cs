using System.ComponentModel.DataAnnotations;

namespace Vint.Core.Database.Models;

public class Graffiti {
    Graffiti(long id) => Id = id;

    public Graffiti(Player player, long id) : this(id) {
        Player = player;
        PlayerId = player.Id;
    }

    [Key] public long Id { get; private set; }

    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}