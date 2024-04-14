using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1486041253393)]
public abstract class EffectBaseTemplate : EntityTemplate {
    protected IEntity Create(string configPath, BattlePlayer battlePlayer, TimeSpan duration, bool withTeam) => Entity(configPath,
        builder => builder
            .AddComponent<EffectComponent>()
            .AddGroupComponent<TankGroupComponent>(battlePlayer.Tank!.Tank)
            .ThenExecuteIf(_ => duration > TimeSpan.Zero,
                entity => {
                    entity.AddComponent(new DurationConfigComponent(duration));
                    entity.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));
                })
            .ThenExecuteIf(_ => withTeam && battlePlayer.Battle.ModeHandler is TeamHandler,
                entity => {
                    entity.AddComponentFrom<TeamGroupComponent>(battlePlayer.Team!);
                    entity.AddComponentFrom<TeamColorComponent>(battlePlayer.Team!);
                }));
}