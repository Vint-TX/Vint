using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1538451741218)]
public class JumpEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration, float force) {
        IEntity entity = Create("battle/effect/jumpimpact", battlePlayer, duration, false);

        entity.AddComponent(new JumpEffectConfigComponent(force));
        return entity;
    }
}
