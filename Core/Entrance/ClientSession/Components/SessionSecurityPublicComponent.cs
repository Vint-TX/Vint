using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.ClientSession.Components;

[ProtocolId(1439792100478)]
public class SessionSecurityPublicComponent(
    string publicKey
) : IComponent {
    public string PublicKey { get; private set; } = publicKey;
}
