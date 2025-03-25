using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Modules.Unit.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.Battle.Weapons.Components.Stream;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Weapons.Templates;

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
