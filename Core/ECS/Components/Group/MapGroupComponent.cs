using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(-9076289125000703482)]
public class MapGroupComponent(
    long key
) : GroupComponent(key) {
    public MapGroupComponent(IEntity entity) : this(entity.Id) { }
}