using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(-1911920453295891173)]
public class CTFTemplate : BattleModeTemplate {
    public override IEntity Create(BattleProperties properties, IEntity lobby, IEntity round, DateTimeOffset startTime) {
        IEntity entity = Entity(properties, lobby, round, startTime);

        entity.AddComponent<CTFComponent>();
        entity.AddComponent<TeamBattleComponent>();
        entity.AddComponent<BattleScoreComponent>();
        return entity;
    }
}
