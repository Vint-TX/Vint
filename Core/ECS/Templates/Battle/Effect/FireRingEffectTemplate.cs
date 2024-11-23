using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1542694831168)]
public class FireRingEffectTemplate : EffectBaseTemplate {
    public IEntity Create(
        BattlePlayer battlePlayer,
        TimeSpan duration,
        bool canTargetTeammates,
        float impactForce,
        float minSplashDamagePercent,
        float radiusOfMinSplashDamage,
        float radiusOfMaxSplashDamage) {
        IEntity entity = Create("battle/effect/firering", battlePlayer, duration, true, true);

        entity.AddComponent<FireRingEffectComponent>();
        entity.AddComponent<DiscreteWeaponComponent>();

        entity.AddComponent(new SplashImpactComponent(impactForce));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        return entity;
    }
}
