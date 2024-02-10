using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-2537616944465628484)]
public class ShaftBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        const string configPath = "garage/weapon/shaft";
        IEntity entity = base.Create(configPath, tank, battlePlayer);

        entity.AddComponent(new ShaftComponent());
        entity.AddComponent(ConfigManager.GetComponent<ShaftEnergyComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<ShaftAimingSpeedComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<ShaftAimingImpactComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<ShaftStateConfigComponent>(configPath.Replace("garage", "battle")));
        return entity;
    }
}