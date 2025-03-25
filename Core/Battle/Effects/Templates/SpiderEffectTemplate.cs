using System.Numerics;
using Vint.Core.Battle.Effects.Components;
using Vint.Core.Battle.Effects.Components.Impl;
using Vint.Core.Battle.Effects.Components.Impl.Mine;
using Vint.Core.Battle.Modules.Unit.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components.Splash;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1485337553359)]
public class SpiderEffectTemplate : EffectBaseTemplate {
    public IEntity Create(
        Tanker tanker,
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
        IEntity entity = Create("battle/effect/spidermine", tanker, duration, true, true);

        entity.AddComponent(new MineConfigComponent(beginHideDistance, hideRange));
        entity.AddComponent(new SpiderMineConfigComponent(speed, acceleration));

        entity.AddComponent(new SplashImpactComponent(impact));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));

        entity.AddComponent<UnitComponent>();
        entity.AddComponent(new UnitMoveComponent(position, tanker.Tank.Orientation));
        entity.AddComponent(new UnitTargetingConfigComponent(targetingDistance));

        entity.AddComponent<DiscreteWeaponComponent>();
        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));

        entity.AddComponentFrom<UserGroupComponent>(tanker.BattleUser);
        entity.AddGroupComponent<UnitGroupComponent>();
        return entity;
    }
}
