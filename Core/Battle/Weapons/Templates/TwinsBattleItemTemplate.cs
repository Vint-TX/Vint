using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(583528765588657091)]
public class TwinsBattleItemTemplate : BulletWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        IEntity entity = base.Create("garage/weapon/twins", tank, tanker);

        entity.AddComponent<TwinsComponent>();
        return entity;
    }
}
