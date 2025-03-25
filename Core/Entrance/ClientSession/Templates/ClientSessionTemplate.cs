using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Entrance.ClientSession.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.ClientSession.Templates;

[ProtocolId(1429771189777)]
public class ClientSessionTemplate : EntityTemplate {
    public IEntity Create() => Entity(null,
        builder => builder
            .AddComponent<ClientSessionComponent>()
            .AddComponent(new SessionSecurityPublicComponent(new Encryption().PublicKey)));
}
