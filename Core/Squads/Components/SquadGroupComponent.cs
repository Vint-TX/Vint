using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Components;

[ProtocolId(1507120787784)]
public class SquadGroupComponent(
    long key
) : GroupComponent(key) {
    public SquadGroupComponent(IEntity entity) : this(entity.Id) { }
}
