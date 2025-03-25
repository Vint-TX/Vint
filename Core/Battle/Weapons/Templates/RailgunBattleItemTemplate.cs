using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Railgun;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-6419489500262573655)]
public class RailgunBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        const string configPath = "garage/weapon/railgun";
        IEntity entity = Create(configPath, tank, tanker);

        entity.AddComponent<RailgunChargingWeaponComponent>(configPath);
        entity.AddComponent<DamageWeakeningByTargetComponent>(configPath);
        entity.AddComponent<RailgunComponent>();
        return entity;
    }
}
