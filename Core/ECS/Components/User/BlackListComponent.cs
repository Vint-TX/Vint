using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1507197930106)]
public class BlackListComponent(
    params long[] blockedUsers
) : IComponent {
    public List<long> BlockedUsers { get; private set; } = blockedUsers.ToList();
}