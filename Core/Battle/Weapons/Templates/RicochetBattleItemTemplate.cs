using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.Battle.Weapons.Components.Config;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.Config;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(-8939173357737272930)]
public class RicochetBattleItemTemplate : BulletWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        const string configPath = "garage/weapon/ricochet";
        IEntity entity = Create(configPath, tank, tanker);

        float energyChargePerShot = ConfigManager.GetComponent<EnergyChargePerShotPropertyComponent>(configPath).FinalValue;
        float energyRechargeSpeed = ConfigManager.GetComponent<EnergyRechargeSpeedPropertyComponent>(configPath).FinalValue;

        entity.AddComponent<RicochetComponent>();
        entity.AddComponent(new DiscreteWeaponEnergyComponent(energyRechargeSpeed, energyChargePerShot));
        return entity;
    }
}
