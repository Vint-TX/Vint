using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Mode.Team.Components;

[ProtocolId(6955808089218759626)]
public class TeamGroupComponent(
    long key
) : GroupComponent(key) {
    public TeamGroupComponent(IEntity entity) : this(entity.Id) { }
}
