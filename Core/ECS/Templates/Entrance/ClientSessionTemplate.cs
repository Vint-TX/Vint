using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Entrance;

[ProtocolId(1429771189777)]
public class ClientSessionTemplate : IEntityTemplate {
    public IEntity Create() => (this as IEntityTemplate).Entity(null,
        builder => builder
            .AddComponent(new ClientSessionComponent())
            .AddComponent(new SessionSecurityPublicComponent(
                "AKhRLW42cTisWpRwUs3EfgLbs1xj32NVOEPwzMUdQiAHWEowCbIBSi0W45P550kTy0U6WKLl3MfFC+bTZ6voG15d:AQAB"))
            .AddComponent(new InviteComponent(null, false)));
}