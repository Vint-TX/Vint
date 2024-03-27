using Vint.Core.Battles;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Mode;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Mode;

[ProtocolId(-1911920453295891173)]
public class CTFTemplate : BattleModeTemplate {
    public override IEntity Create(TypeHandler typeHandler, IEntity lobby, int scoreLimit, int timeLimit, int userLimit, int warmUpTimeLimit) {
        IEntity entity = Entity(typeHandler, lobby, BattleMode.CTF, scoreLimit, timeLimit, userLimit, warmUpTimeLimit);

        entity.AddComponent<CTFComponent>();
        entity.AddComponent<TeamBattleComponent>();
        entity.AddComponent<BattleScoreComponent>();
        return entity;
    }
}