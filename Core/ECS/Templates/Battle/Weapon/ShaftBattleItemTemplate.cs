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
        entity.AddComponent(new ShaftEnergyComponent(0.2857f,
            1,
            0.2f,
            0.143f)); // values from https://github.com/Assasans/TXServer-Public/blob/826161beae3ecbd7bb32403f0109752716bdf965/TXServer/ECSSystem/EntityTemplates/Battle/Weapon/ShaftBattleItemTemplate.cs#L22
        entity.AddComponent(ConfigManager.GetComponent<ShaftAimingImpactComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<ShaftAimingSpeedComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<ShaftStateConfigComponent>(configPath.Replace("garage", "battle")));
        return entity;
    }
}