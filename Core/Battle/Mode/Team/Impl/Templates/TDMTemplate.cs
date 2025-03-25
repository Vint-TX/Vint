using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(8215935014037697786)]
public class TDMTemplate : BattleModeTemplate {
    public override IEntity Create(BattleProperties properties, IEntity lobby, IEntity round, DateTimeOffset startTime) {
        IEntity entity = Entity(properties, lobby, round, startTime);

        entity.AddComponent<TDMComponent>();
        entity.AddComponent<TeamBattleComponent>();
        entity.AddComponent<BattleScoreComponent>();
        return entity;
    }
}
