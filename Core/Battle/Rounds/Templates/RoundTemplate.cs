using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Rounds.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rounds.Templates;

[ProtocolId(1429256309752)]
public class RoundTemplate : EntityTemplate {
    public IEntity Create(DateTimeOffset stopTime) => Entity(null,
        builder => builder
            .AddComponent<RoundComponent>()
            .AddComponent<RoundActiveStateComponent>()
            .AddComponent(new RoundStopTimeComponent(stopTime))
            .AddGroupComponent<BattleGroupComponent>());
}
