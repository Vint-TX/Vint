using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1507197930106)]
public class BlackListComponent(
    params List<long> blockedUsers
) : PrivateComponent {
    public List<long> BlockedUsers { get; } = blockedUsers.ToList();
}
