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
    protected IEntity Create(string configPath, BattlePlayer battlePlayer, TimeSpan duration, bool withTeam) {
        IEntity entity = Entity(configPath,
            builder => builder
                .AddComponent(new EffectComponent())
                .AddComponent(battlePlayer.Tank!.Tank.GetComponent<TankGroupComponent>()));

        if (duration > TimeSpan.Zero) {
            entity.AddComponent(new DurationConfigComponent(duration));
            entity.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));
        }

        if (withTeam && battlePlayer.Battle.ModeHandler is TeamHandler) {
            entity.AddComponent(battlePlayer.Team!.GetComponent<TeamGroupComponent>());
            entity.AddComponent(battlePlayer.Team!.GetComponent<TeamColorComponent>());
        }

        return entity;
    }
}