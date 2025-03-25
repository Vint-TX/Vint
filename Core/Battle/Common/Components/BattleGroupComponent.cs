using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Common.Components;

[ProtocolId(1140613249019529884)]
public class BattleGroupComponent(
    long key
) : GroupComponent(key) {
    public BattleGroupComponent(IEntity entity) : this(entity.Id) { }
}
