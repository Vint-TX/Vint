using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1482920154068)]
public class UserSubscribeComponent(
    bool subscribed
) : PrivateComponent {
    public bool Subscribed => subscribed;
}
