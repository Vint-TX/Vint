using Vint.Core.Battle.Effects.Components;
using Vint.Core.Battle.Effects.Components.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components.Splash;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1542694831168)]
public class FireRingEffectTemplate : EffectBaseTemplate {
    public IEntity Create(
        Tanker tanker,
        TimeSpan duration,
        bool canTargetTeammates,
        float impactForce,
        float minSplashDamagePercent,
        float radiusOfMinSplashDamage,
        float radiusOfMaxSplashDamage) {
        IEntity entity = Create("battle/effect/firering", tanker, duration, true, true);

        entity.AddComponent<FireRingEffectComponent>();
        entity.AddComponent<DiscreteWeaponComponent>();

        entity.AddComponent(new SplashImpactComponent(impactForce));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        return entity;
    }
}
