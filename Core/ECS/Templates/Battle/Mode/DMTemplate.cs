using Vint.Core.Battles;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(-4141404049750078994)]
public class DMTemplate : BattleModeTemplate {
    public override IEntity Create(TypeHandler typeHandler, IEntity lobby, int scoreLimit, int timeLimit, int userLimit, int warmUpTimeLimit) {
        IEntity entity = Entity(typeHandler, lobby, BattleMode.DM, scoreLimit, timeLimit, userLimit, warmUpTimeLimit);

        entity.AddComponent<DMComponent>();
        return entity;
    }
}