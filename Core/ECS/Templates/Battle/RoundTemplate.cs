using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle;

[ProtocolId(1429256309752)]
public class RoundTemplate : EntityTemplate {
    public IEntity Create(DateTimeOffset stopTime) => Entity(null,
        builder => builder
            .AddComponent<RoundComponent>()
            .AddComponent<RoundActiveStateComponent>()
            .AddComponent(new RoundStopTimeComponent(stopTime))
            .AddGroupComponent<BattleGroupComponent>());
}
