using Vint.Core.Battle.Effects.Components.Impl.EMP;
using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(636250001674528714)]
public class EMPEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration, float radius) {
        IEntity entity = Create("battle/effect/emp", tanker, duration, false, false);

        entity.AddComponent(new EMPEffectComponent(radius));
        return entity;
    }
}
