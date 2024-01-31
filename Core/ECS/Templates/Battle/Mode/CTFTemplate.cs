using Vint.Core.Battles;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(-1911920453295891173)]
public class CTFTemplate : BattleModeTemplate {
    public override IEntity Create(IEntity lobby, int scoreLimit, int timeLimit, int warmUpTimeLimit) {
        IEntity entity = Entity(lobby, BattleMode.CTF, scoreLimit, timeLimit, warmUpTimeLimit);

        entity.AddComponent(new CTFComponent());
        entity.AddComponent(new TeamBattleComponent());
        entity.AddComponent(new BattleScoreComponent());
        return entity;
    }
}