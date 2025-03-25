using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-8770103861152493981)]
public class ThunderBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        const string configPath = "garage/weapon/thunder";
        IEntity entity = Create(configPath, tank, tanker);

        entity.AddComponent<ThunderComponent>();
        entity.AddComponent<SplashImpactComponent>(configPath);
        entity.AddComponent<SplashWeaponComponent>(configPath.Replace("garage", "battle"));
        return entity;
    }
}
