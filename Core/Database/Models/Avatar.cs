using Vint.Core.ECS;

namespace Vint.Core.Database.Models;

public class Avatar {
    Avatar(long avatarId) => AvatarId = avatarId;

    public Avatar(Player player, long avatarId) : this(avatarId) {
        Player = player;
        PlayerId = player.Id;
    }

    public long AvatarId { get; private set; }

    public Player Player { get; private set; } = null!;
    public uint PlayerId { get; private set; }
}
