using Vint.Core.Battles;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(-4141404049750078994)]
public class DMTemplate : BattleModeTemplate {
    public override IEntity Create(IEntity lobby, int scoreLimit, int timeLimit, int warmUpTimeLimit) {
        IEntity entity = Entity(lobby, BattleMode.DM, scoreLimit, timeLimit, warmUpTimeLimit);

        entity.AddComponent(new DMComponent());
        return entity;
    }
}