using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(-2434344547754767853)]
public class SmokyBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        IEntity entity = base.Create("garage/weapon/smoky", tank, tanker);

        entity.AddComponent<SmokyComponent>();
        return entity;
    }
}
