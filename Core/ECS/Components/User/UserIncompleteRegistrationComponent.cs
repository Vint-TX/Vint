using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1482675132842)]
public class UserIncompleteRegistrationComponent : IComponent {
    public bool FirstBattleDone { get; private set; }
}