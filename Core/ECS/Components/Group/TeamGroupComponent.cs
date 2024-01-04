using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(6955808089218759626)]
public class TeamGroupComponent(
    long key
) : GroupComponent(key) {
    public TeamGroupComponent(IEntity entity) : this(entity.Id) { }
}