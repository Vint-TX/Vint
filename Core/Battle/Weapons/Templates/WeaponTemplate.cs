using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(1430285417001)]
public abstract class WeaponTemplate : EntityTemplate {
    protected virtual IEntity Create(string configPath, IEntity tank, Tanker tanker) => Entity(configPath.Replace("garage", "battle"),
        builder => builder
            .AddComponent<WeaponComponent>()
            .AddComponent<WeaponRotationComponent>(tank.TemplateAccessor!.ConfigPath!.Replace("battle", "garage"))
            .TryAddComponent<WeaponCooldownComponent>(configPath)
            .AddComponentFrom<TankPartComponent>(tank)
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .AddComponentFrom<MarketItemGroupComponent>(tanker.Connection.Player.CurrentPreset.Weapon)
            .AddGroupComponent<TankGroupComponent>(tank)
            .ThenExecuteIf(_ => this is not HammerBattleItemTemplate, entity => entity.AddComponent(new WeaponEnergyComponent(1)))
            .ThenExecuteIf(_ => tanker.Team != null, entity => entity.AddGroupComponent<TeamGroupComponent>(tanker.Team!)));
}
