using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components.Stream;
using Vint.Core.Battle.Weapons.Components.Vulcan;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(-3936735916503799349)]
public class VulcanBattleItemTemplate : StreamWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        const string configPath = "garage/weapon/vulcan";
        IEntity entity = Create(configPath, tank, tanker);

        entity.AddComponent<VulcanComponent>();
        entity.AddComponent<StreamHitConfigComponent>("battle/weapon/vulcan");
        entity.AddComponent<VulcanWeaponComponent>(configPath);
        entity.AddComponent<KickbackComponent>(configPath);
        entity.AddComponent<ImpactComponent>(configPath);
        return entity;
    }
}
