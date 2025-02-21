using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.User;

[ProtocolId(1496906087610)]
public class UserEquipmentComponent(
    long weaponId,
    long hullId
) : IComponent {
    public UserEquipmentComponent(IEntity weapon, IEntity hull) : this(weapon.Id, hull.Id) { }

    public long WeaponId { get; private set; } = weaponId;
    public long HullId { get; private set; } = hullId;
}
