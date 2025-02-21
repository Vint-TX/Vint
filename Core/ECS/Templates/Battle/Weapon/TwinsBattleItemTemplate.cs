using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(583528765588657091)]
public class TwinsBattleItemTemplate : BulletWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        IEntity entity = base.Create("garage/weapon/twins", tank, tanker);

        entity.AddComponent<TwinsComponent>();
        return entity;
    }
}
