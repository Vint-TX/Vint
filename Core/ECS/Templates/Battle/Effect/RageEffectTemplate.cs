using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(636364997672415488)]
public class RageEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration, TimeSpan decreaseCooldownPerKill) {
        IEntity entity = Create("battle/effect/rage", battlePlayer, duration, false);
        
        entity.AddComponent(new RageEffectComponent(decreaseCooldownPerKill));
        return entity;
    }
}