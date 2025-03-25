using Vint.Core.Battle.Effects.Components;
using Vint.Core.Battle.Effects.Components.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components.Splash;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1542270967570)]
public class ExternalImpactEffectTemplate : EffectBaseTemplate {
    public IEntity Create(
        Tanker tanker,
        TimeSpan duration,
        bool canTargetTeammates,
        float impactForce,
        float minSplashDamagePercent,
        float radiusOfMaxSplashDamage,
        float radiusOfMinSplashDamage) {
        IEntity entity = Create("battle/effect/externalimpact", tanker, duration, true, true);

        entity.AddComponent<ExternalImpactEffectComponent>();
        entity.AddComponent<DiscreteWeaponComponent>();

        entity.AddComponent(new SplashImpactComponent(impactForce));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        return entity;
    }
}
