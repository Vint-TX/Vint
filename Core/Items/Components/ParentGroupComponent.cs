using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Components;

[ProtocolId(635908808598551080)]
public class ParentGroupComponent(
    long key
) : GroupComponent(key) {
    public ParentGroupComponent(IEntity entity) : this(entity.Id) { }
}
