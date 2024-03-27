using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle;

[ProtocolId(1429256309752)]
public class RoundTemplate : EntityTemplate {
    public IEntity Create(IEntity battle) => Entity(null,
        builder => builder
            .AddComponent<RoundComponent>()
            .AddComponent(new RoundStopTimeComponent(DateTimeOffset.UtcNow.AddSeconds(40)))
            .AddComponent<RoundActiveStateComponent>()
            .AddComponentFrom<BattleGroupComponent>(battle));
}