using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1543482696222)]
public class ExplosiveMassEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration, float radius, float delay) {
        IEntity entity = Create("battle/effect/externalimpact", tanker, duration, true, true);

        entity.AddComponent<ExternalImpactEffectComponent>();
        entity.AddComponent<DiscreteWeaponComponent>();

        entity.AddComponent(new ExplosiveMassEffectComponent(radius, delay));
        return entity;
    }
}
