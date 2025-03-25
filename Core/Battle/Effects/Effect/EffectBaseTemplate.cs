using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

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
