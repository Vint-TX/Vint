using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(9171752353079252620)]
public class UserMoneyComponent(
    long money
) : PrivateComponent {
    public long Money { get; set; } = money;
}
