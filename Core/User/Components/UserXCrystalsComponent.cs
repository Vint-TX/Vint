using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1473074767785)]
public class UserXCrystalsComponent(
    long money
) : PrivateComponent {
    public long Money { get; set; } = money;
}
