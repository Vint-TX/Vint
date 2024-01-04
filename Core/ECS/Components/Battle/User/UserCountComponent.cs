using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.User;

[ProtocolId(1436520497855)]
public class UserCountComponent(
    int userCount
) : IComponent {
    public int UserCount { get; private set; } = userCount;
}