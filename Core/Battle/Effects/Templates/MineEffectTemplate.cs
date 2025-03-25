using System.Numerics;
using Vint.Core.Battle.Effects.Components;
using Vint.Core.Battle.Effects.Components.Impl.Mine;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components.Splash;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1486709084156)]
public class MineEffectTemplate : EffectBaseTemplate {
    protected virtual string ConfigPath => "battle/effect/mine";

    public IEntity Create(
        Tanker tanker,
        TimeSpan duration,
        Vector3 position,
        bool canTargetTeammates,
        float beginHideDistance,
        float hideRange,
        float triggeringArea,
        float impact,
        float minSplashDamagePercent,
        float radiusOfMaxSplashDamage,
        float radiusOfMinSplashDamage) {
        IEntity entity = Create(ConfigPath, tanker, duration, true, true);

        entity.AddComponent(new MineConfigComponent(beginHideDistance, hideRange));
        entity.AddComponent(new MineEffectTriggeringAreaComponent(triggeringArea));
        entity.AddComponent(new MinePositionComponent(position));

        entity.AddComponent(new SplashImpactComponent(impact));
        entity.AddComponent(new SplashEffectComponent(canTargetTeammates));
        entity.AddComponent(new SplashWeaponComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));

        entity.AddComponent(new DamageWeakeningByDistanceComponent(minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage));
        entity.AddComponent<DiscreteWeaponComponent>();
        entity.AddComponentFrom<UserGroupComponent>(tanker.BattleUser);
        return entity;
    }
}
