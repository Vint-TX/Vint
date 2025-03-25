using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Stream;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(1485335125183)]
public class DroneWeaponTemplate : EntityTemplate {
    public IEntity Create(Tanker tanker) =>
        Entity("battle/effect/droneweapon",
            builder => builder
                .AddComponent<WeaponComponent>()
                .AddComponent<DroneWeaponComponent>()
                .AddComponentFrom<UserGroupComponent>(tanker.BattleUser)
                .AddGroupComponent<UnitGroupComponent>()
                .AddComponentFromConfig<WeaponCooldownComponent>()
                .AddComponentFromConfig<StreamHitConfigComponent>()
                .ThenExecuteIf(_ => tanker.Team != null, entity => entity.AddGroupComponent<TeamGroupComponent>(tanker.Team)));
}
