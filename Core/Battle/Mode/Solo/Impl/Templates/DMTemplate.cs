using Vint.Core.Battle.Mode.Common.Templates;
using Vint.Core.Battle.Mode.Solo.Impl.Components;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Properties.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Mode.Solo.Impl.Templates;

[ProtocolId(-4141404049750078994)]
public class DMTemplate : BattleModeTemplate {
    public override IEntity Create(BattleProperties properties, IEntity lobby, IEntity round, DateTimeOffset startTime) {
        IEntity entity = Entity(properties, lobby, round, startTime);

        entity.AddComponent<DMComponent>();
        entity.AddComponent<ScoreLimitComponent>();
        return entity;
    }
}
