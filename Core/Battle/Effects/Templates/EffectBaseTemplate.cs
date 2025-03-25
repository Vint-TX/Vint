using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Effects.Components;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1486041253393)]
public abstract class EffectBaseTemplate : EntityTemplate {
    protected IEntity Create(string configPath, Tanker tanker, TimeSpan duration, bool withTeam, bool withBattle) => Entity(configPath,
        builder => builder
            .AddComponent<EffectComponent>()
            .AddGroupComponent<TankGroupComponent>(tanker.Tank.Entities.Tank)
            .ThenExecuteIf(_ => duration > TimeSpan.Zero,
                entity => {
                    entity.AddComponent(new DurationConfigComponent(duration));
                    entity.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));
                })
            .ThenExecuteIf(_ => withTeam && tanker.Team != null, entity => entity.AddGroupComponent<TeamGroupComponent>(tanker.Team))
            .ThenExecuteIf(_ => withBattle, entity => entity.AddGroupComponent<BattleGroupComponent>(tanker.Round.Entity)));
}
