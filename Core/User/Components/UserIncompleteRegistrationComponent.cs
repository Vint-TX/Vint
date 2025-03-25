using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1482675132842)]
public class UserIncompleteRegistrationComponent : IComponent {
    public bool FirstBattleDone { get; private set; }
}
