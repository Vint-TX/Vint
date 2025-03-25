using Vint.Core.Battle.Mode.Common.Templates;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Mode.Team.Impl.Components;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Mode.Team.Impl.Templates;

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
