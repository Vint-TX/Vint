using Vint.Core.ECS.Movement;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Movement;

[ProtocolId(-615965945505672897)]
public class TankMovementComponent(
    ECS.Movement.Movement movement,
    MoveControl moveControl,
    float weaponRotation,
    float weaponControl
) : IComponent {
    public ECS.Movement.Movement Movement { get; set; } = movement;
    public MoveControl MoveControl { get; set; } = moveControl;
    public float WeaponRotation { get; set; } = weaponRotation;
    public float WeaponControl { get; set; } = weaponControl;
}
