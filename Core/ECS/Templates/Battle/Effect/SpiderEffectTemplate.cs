using System.Numerics;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Components.Battle.Effect.Type.Mine;
using Vint.Core.ECS.Components.Battle.Unit;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1485337553359)]
public class SpiderEffectTemplate : EffectBaseTemplate {
    public IEntity Create(
        BattlePlayer battlePlayer,
        TimeSpan duration,
        Vector3 position,
        bool canTargetTeammates,
        float beginHideDistance,
        float hideRange,
        float impact,
        float minSplashDamagePercent,
        float radiusOfMaxSplashDamage,
        float radiusOfMinSplashDamage,
        float targetingDistance,
        float speed,
        float acceleration) {
        IEntity entity = Create("battle/effect/spidermine", battlePlayer, duration, true, true);

        entity.AddComponent(new MineConfigComponent(beginHideDistance, hideRange));
        entity.AddComponent(new SpiderMineConfigComponent(speed, acceleration));

        entity.AddComponent(new SplashImpactComponent(impact));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));

        entity.AddComponent<UnitComponent>();
        entity.AddComponent(new UnitMoveComponent(position, battlePlayer.Tank!.Orientation));
        entity.AddComponent(new UnitTargetingConfigComponent(targetingDistance));

        entity.AddComponent<DiscreteWeaponComponent>();
        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));

        entity.AddComponentFrom<UserGroupComponent>(battlePlayer.BattleUser);
        entity.AddGroupComponent<UnitGroupComponent>();
        return entity;
    }
}
