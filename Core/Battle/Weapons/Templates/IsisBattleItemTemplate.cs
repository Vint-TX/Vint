using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.Battle.Weapons.Components.Stream;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(3413384256910001471)]
public class IsisBattleItemTemplate : StreamWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        IEntity entity = base.Create("garage/weapon/isis", tank, tanker);

        entity.AddComponent<IsisComponent>();
        entity.AddComponent<StreamHitConfigComponent>("battle/weapon/isis");
        return entity;
    }
}
