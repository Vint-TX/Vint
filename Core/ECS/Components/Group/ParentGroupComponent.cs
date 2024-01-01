using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(635908808598551080)]
public class ParentGroupComponent(
    long key
) : GroupComponent(key) {
    public ParentGroupComponent(IEntity entity) : this(entity.Id) { }
}