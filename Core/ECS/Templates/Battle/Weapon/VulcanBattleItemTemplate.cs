using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Battle.Weapon.Stream;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-3936735916503799349)]
public class VulcanBattleItemTemplate : StreamWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        const string configPath = "garage/weapon/vulcan";
        IEntity entity = Create(configPath, tank, battlePlayer);

        entity.AddComponent<VulcanComponent>();
        entity.AddComponent<StreamHitConfigComponent>("battle/weapon/vulcan");
        entity.AddComponent<VulcanWeaponComponent>(configPath);
        entity.AddComponent<KickbackComponent>(configPath);
        entity.AddComponent<ImpactComponent>(configPath);
        return entity;
    }
}
