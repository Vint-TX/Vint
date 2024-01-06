using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Lobby;

[ProtocolId(3911401339075883957)]
public class UserLimitComponent(
    int lobbyLimit,
    int teamLimit
) : IComponent {
    public UserLimitComponent(int lobbyLimit) : this(lobbyLimit, lobbyLimit / 2) { }

    public int UserLimit { get; private set; } = lobbyLimit;
    public int TeamLimit { get; private set; } = teamLimit;
}