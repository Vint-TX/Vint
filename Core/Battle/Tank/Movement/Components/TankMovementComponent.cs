using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Movement.Components;

[ProtocolId(-615965945505672897)]
public class TankMovementComponent(
    Movement movement,
    MoveControl moveControl,
    float weaponRotation,
    float weaponControl
) : IComponent {
    public Movement Movement { get; set; } = movement;
    public MoveControl MoveControl { get; set; } = moveControl;
    public float WeaponRotation { get; set; } = weaponRotation;
    public float WeaponControl { get; set; } = weaponControl;
}
