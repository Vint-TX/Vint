using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(9171752353079252620)]
public class UserMoneyComponent(
    long money
) : IComponent {
    public long Money { get; set; } = money;
}