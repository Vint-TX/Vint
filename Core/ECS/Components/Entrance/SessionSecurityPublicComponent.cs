using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Entrance;

[ProtocolId(1439792100478)]
public class SessionSecurityPublicComponent(
    string publicKey
) : IComponent {
    public string PublicKey { get; private set; } = publicKey;
}