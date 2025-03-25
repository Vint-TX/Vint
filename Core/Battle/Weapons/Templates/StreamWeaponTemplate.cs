using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components.Stream;
using Vint.Core.Config;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(1430285569243)]
public abstract class StreamWeaponTemplate : WeaponTemplate {
    protected override IEntity Create(string configPath, IEntity tank, Tanker tanker) {
        IEntity entity = base.Create(configPath, tank, tanker);

        if (ConfigManager.TryGetComponent(configPath, out StreamWeaponEnergyComponent? streamWeaponEnergyComponent))
            entity.AddComponent(streamWeaponEnergyComponent);

        entity.AddComponent<StreamWeaponComponent>();
        return entity;
    }
}
