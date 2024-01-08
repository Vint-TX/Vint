using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1473074767785)]
public class UserXCrystalsComponent(
    long money
) : IComponent {
    public long Money { get; set; } = money;
}