using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-2537616944465628484)]
public class ShaftBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        const string configPath = "garage/weapon/shaft";
        IEntity entity = base.Create(configPath, tank, battlePlayer);

        entity.AddComponent<ShaftComponent>();
        entity.AddComponent<ShaftEnergyComponent>(configPath);
        entity.AddComponent<ShaftAimingSpeedComponent>(configPath);
        entity.AddComponent<ShaftAimingImpactComponent>(configPath);
        entity.AddComponent<ShaftStateConfigComponent>(configPath.Replace("garage", "battle"));
        return entity;
    }
}