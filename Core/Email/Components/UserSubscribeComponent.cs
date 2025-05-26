using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Components;

[ProtocolId(1482920154068)]
public class UserSubscribeComponent(
    bool subscribed
) : PrivateComponent {
    public bool Subscribed { get; set; } = subscribed;
}
