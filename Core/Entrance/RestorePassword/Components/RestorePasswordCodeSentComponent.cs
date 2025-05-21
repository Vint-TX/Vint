using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Components;

[ProtocolId(1479198715562)]
public class RestorePasswordCodeSentComponent(
    string email
) : IComponent {
    public string Email => email;
}
