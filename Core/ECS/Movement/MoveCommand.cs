namespace Vint.Core.ECS.Movement;

public struct MoveCommand {
    public Movement? Movement { get; set; }
    public float? WeaponRotation { get; set; }
    public float? TankControlVertical { get; set; }
    public float? TankControlHorizontal { get; set; }
    public float? WeaponRotationControl { get; set; }

    public int ClientTime { get; set; }

    public bool IsDiscrete() => IsFloatDiscrete(TankControlVertical) &&
                                IsFloatDiscrete(TankControlHorizontal) &&
                                IsFloatDiscrete(WeaponRotationControl);

    static bool IsFloatDiscrete(float? val) => val is 0f or 1f or -1f;
}