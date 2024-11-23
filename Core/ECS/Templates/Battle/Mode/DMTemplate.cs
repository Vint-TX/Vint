using Vint.Core.Battles;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(-4141404049750078994)]
public class DMTemplate : BattleModeTemplate {
    public override IEntity Create(TypeHandler typeHandler, IEntity lobby, int timeLimit, int userLimit, int warmUpTimeLimit) {
        IEntity entity = Entity(typeHandler, lobby, BattleMode.DM, timeLimit, userLimit, warmUpTimeLimit);

        entity.AddComponent<DMComponent>();
        entity.AddComponent<ScoreLimitComponent>();
        return entity;
    }
}
