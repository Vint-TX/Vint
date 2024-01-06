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
    public ECS.Movement.Movement Movement { get; private set; } = movement;
    public MoveControl MoveControl { get; private set; } = moveControl;
    public float WeaponRotation { get; private set; } = weaponRotation;
    public float WeaponControl { get; private set; } = weaponControl;
}