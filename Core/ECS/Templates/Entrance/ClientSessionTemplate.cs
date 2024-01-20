using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Templates.Entrance;

[ProtocolId(1429771189777)]
public class ClientSessionTemplate : EntityTemplate {
    public IEntity Create() => Entity(null,
        builder =>
            builder
                .AddComponent(new ClientSessionComponent())
                .AddComponent(new SessionSecurityPublicComponent(new Encryption().PublicKey))
                .AddComponent(new InviteComponent(null, false)));
}