using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1543482696222)]
public class ExplosiveMassEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration, float radius, float delay) {
        IEntity entity = Create("battle/effect/externalimpact", battlePlayer, duration, true);

        entity.AddComponent<ExternalImpactEffectComponent>();
        entity.AddComponent<DiscreteWeaponComponent>();

        entity.AddComponent(new ExplosiveMassEffectComponent(radius, delay));

        entity.AddGroupComponent<BattleGroupComponent>(battlePlayer.Battle.Entity);
        return entity;
    }
}
