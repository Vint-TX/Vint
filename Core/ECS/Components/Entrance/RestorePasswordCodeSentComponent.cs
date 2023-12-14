using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Entrance;

[ProtocolId(1479198715562)]
public class RestorePasswordCodeSentComponent(string email) : IComponent {
    public string Email => email;
}