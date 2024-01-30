using Vint.Core.Battles;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(8215935014037697786)]
public class TDMTemplate : BattleModeTemplate {
    public override IEntity Create(IEntity lobby, int scoreLimit, int timeLimit, int warmUpTimeLimit) {
        IEntity entity = Entity(lobby, BattleMode.TDM, scoreLimit, timeLimit, warmUpTimeLimit);

        entity.AddComponent(new TDMComponent());
        entity.AddComponent(new TeamBattleComponent());
        entity.AddComponent(new BattleScoreComponent());
        return entity;
    }
}