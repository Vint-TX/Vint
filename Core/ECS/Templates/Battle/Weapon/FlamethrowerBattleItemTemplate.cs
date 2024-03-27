using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(4652768934679402653)]
public class FlamethrowerBattleItemTemplate : StreamWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        IEntity entity = Create("garage/weapon/flamethrower", tank, battlePlayer);

        entity.AddComponent<FlamethrowerComponent>();
        return entity;
    }
}