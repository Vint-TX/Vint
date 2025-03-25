using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(4652768934679402653)]
public class FlamethrowerBattleItemTemplate : StreamWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        IEntity entity = Create("garage/weapon/flamethrower", tank, tanker);

        entity.AddComponent<FlamethrowerComponent>();
        return entity;
    }
}
