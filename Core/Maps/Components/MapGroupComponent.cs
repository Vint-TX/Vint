using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Maps.Components;

[ProtocolId(-9076289125000703482)]
public class MapGroupComponent(
    long key
) : GroupComponent(key) {
    public MapGroupComponent(IEntity entity) : this(entity.Id) { }
}
