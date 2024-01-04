using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1486736500959)]
public class NotificationGroupComponent(
    long key
) : GroupComponent(key) {
    public NotificationGroupComponent(IEntity entity) : this(entity.Id) { }
}