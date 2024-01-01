using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1482920154068)]
public class UserSubscribeComponent(
    bool subscribed
) : IComponent {
    public bool Subscribed => subscribed;
}