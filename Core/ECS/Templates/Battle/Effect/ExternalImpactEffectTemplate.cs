using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Templates.Battle.Effect;

public class ExternalImpactEffectTemplate : EffectBaseTemplate {
    public IEntity Create(
        BattlePlayer battlePlayer,
        TimeSpan duration,
        bool canTargetTeammates,
        float impactForce,
        float minSplashDamagePercent,
        float radiusOfMaxSplashDamage,
        float radiusOfMinSplashDamage) {
        IEntity entity = Create("battle/effect/externalimpact", battlePlayer, duration, true);
        
        entity.AddComponent<ExternalImpactEffectComponent>();
        entity.AddComponent<DiscreteWeaponComponent>();
        
        entity.AddComponent(new SplashImpactComponent(impactForce));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        
        entity.AddGroupComponent<BattleGroupComponent>(battlePlayer.Battle.Entity);
        return entity;
    }
}