using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-1716200834009238305)]
public abstract class DiscreteWeaponTemplate : WeaponTemplate {
    protected override IEntity Create(string configPath, IEntity tank, BattlePlayer battlePlayer) {
        IEntity entity = base.Create(configPath, tank, battlePlayer);

        entity.AddComponent(new DiscreteWeaponComponent());
        entity.AddComponent(ConfigManager.GetComponent<ImpactComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<KickbackComponent>(configPath));

        if (ConfigManager.TryGetComponent(configPath, out DamageWeakeningByDistanceComponent? damageWeakeningByDistanceComponent))
            entity.AddComponent(damageWeakeningByDistanceComponent);

        if (this is not RicochetBattleItemTemplate or ShaftBattleItemTemplate)
            entity.AddComponent(ConfigManager.GetComponent<DiscreteWeaponEnergyComponent>(configPath));

        return entity;
    }
}