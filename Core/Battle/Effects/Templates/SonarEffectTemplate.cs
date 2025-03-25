using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(636222333880646188)]
public class SonarEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration) {
        IEntity entity = Create("battle/effect/sonar", tanker, duration, true, false);

        entity.AddGroupComponent<UserGroupComponent>(tanker.Connection.UserContainer.Entity);
        return entity;
    }
}
