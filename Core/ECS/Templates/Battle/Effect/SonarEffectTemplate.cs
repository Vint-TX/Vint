using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(636222333880646188)]
public class SonarEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration) {
        IEntity entity = Create("battle/effect/sonar", tanker, duration, true, false);

        entity.AddGroupComponent<UserGroupComponent>(tanker.Connection.UserContainer.Entity);
        return entity;
    }
}
