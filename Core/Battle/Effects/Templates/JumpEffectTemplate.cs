using Vint.Core.Battle.Effects.Components.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1538451741218)]
public class JumpEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration, float force) {
        IEntity entity = Create("battle/effect/jumpimpact", tanker, duration, false, false);

        entity.AddComponent(new JumpEffectConfigComponent(force));
        return entity;
    }
}
