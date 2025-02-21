using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(-4141404049750078994)]
public class DMTemplate : BattleModeTemplate {
    public override IEntity Create(BattleProperties properties, IEntity lobby, IEntity round, DateTimeOffset startTime) {
        IEntity entity = Entity(properties, lobby, round, startTime);

        entity.AddComponent<DMComponent>();
        entity.AddComponent<ScoreLimitComponent>();
        return entity;
    }
}
