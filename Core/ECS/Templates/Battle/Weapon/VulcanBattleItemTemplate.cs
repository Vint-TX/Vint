using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Stream;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-3936735916503799349)]
public class VulcanBattleItemTemplate : StreamWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        const string configPath = "garage/weapon/vulcan";
        IEntity entity = Create(configPath, tank, battlePlayer);

        entity.AddComponent(new VulcanComponent());
        entity.AddComponent(ConfigManager.GetComponent<StreamHitConfigComponent>("battle/weapon/vulcan"));
        entity.AddComponent(ConfigManager.GetComponent<VulcanWeaponComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<KickbackComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<ImpactComponent>(configPath));

        return entity;
    }
}