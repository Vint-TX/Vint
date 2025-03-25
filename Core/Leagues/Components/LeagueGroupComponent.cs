using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Leagues.Components;

[ProtocolId(1503298026299)]
public class LeagueGroupComponent(
    long key
) : GroupComponent(key) {
    public LeagueGroupComponent(IEntity entity) : this(entity.Id) { }
}
