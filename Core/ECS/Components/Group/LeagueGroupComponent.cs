using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1503298026299)]
public class LeagueGroupComponent(
    long key
) : GroupComponent(key) {
    public LeagueGroupComponent(IEntity entity) : this(entity.Id) { }
}