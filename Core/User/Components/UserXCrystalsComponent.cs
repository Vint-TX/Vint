using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1473074767785)]
public class UserXCrystalsComponent(
    long money
) : PrivateComponent {
    public long Money { get; set; } = money;
}
