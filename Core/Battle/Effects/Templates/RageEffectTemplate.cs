using Vint.Core.Battle.Effects.Components.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(636364997672415488)]
public class RageEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration) {
        IEntity entity = Create("battle/effect/rage", tanker, duration, false, false);

        entity.AddComponent<RageEffectComponent>();
        return entity;
    }
}
