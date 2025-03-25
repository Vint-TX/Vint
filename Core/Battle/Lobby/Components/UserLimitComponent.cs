using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.Components;

[ProtocolId(3911401339075883957)]
public class UserLimitComponent(
    int lobbyLimit,
    int teamLimit
) : IComponent {
    public UserLimitComponent(int lobbyLimit) : this(lobbyLimit, lobbyLimit / 2) { }

    public int UserLimit { get; private set; } = lobbyLimit;
    public int TeamLimit { get; private set; } = teamLimit;
}
