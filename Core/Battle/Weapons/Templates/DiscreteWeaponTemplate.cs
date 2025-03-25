using Vint.Core.Battle.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-1716200834009238305)]
public abstract class DiscreteWeaponTemplate : WeaponTemplate {
    protected override IEntity Create(string configPath, IEntity tank, Tanker tanker) {
        IEntity entity = base.Create(configPath, tank, tanker);

        entity.AddComponent<DiscreteWeaponComponent>();
        entity.AddComponent<ImpactComponent>(configPath);
        entity.AddComponent<KickbackComponent>(configPath);

        if (ConfigManager.TryGetComponent(configPath, out DamageWeakeningByDistanceComponent? damageWeakeningByDistanceComponent))
            entity.AddComponent(damageWeakeningByDistanceComponent);

        if (this is not (RicochetBattleItemTemplate or ShaftBattleItemTemplate))
            entity.AddComponent<DiscreteWeaponEnergyComponent>(configPath);

        return entity;
    }
}
