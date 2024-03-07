namespace Vint.Core.ECS.Components.Server;

public class BonusConfigComponent : IComponent {
    public float FallSpeed { get; private set; }
    public float AngularSpeed { get; private set; }
    public float SwingFreq { get; private set; }
    public float SwingAngle { get; private set; }
    public float AlignmentToGroundAngularSpeed { get; private set; }
    public float AppearingOnGroundTime { get; private set; }
    public float SpawnDuration { get; private set; }
}