using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon.Stream;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(1430285569243)]
public abstract class StreamWeaponTemplate : WeaponTemplate {
    protected override IEntity Create(string configPath, IEntity tank, BattlePlayer battlePlayer) {
        IEntity entity = base.Create(configPath, tank, battlePlayer);

        if (ConfigManager.TryGetComponent(configPath, out StreamWeaponEnergyComponent? streamWeaponEnergyComponent))
            entity.AddComponent(streamWeaponEnergyComponent);

        entity.AddComponent<StreamWeaponComponent>();
        return entity;
    }
}