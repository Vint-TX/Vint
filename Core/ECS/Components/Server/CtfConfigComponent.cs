namespace Vint.Core.ECS.Components.Server;

public class CtfConfigComponent : IComponent {
    public float MinDistanceFromMineToBase { get; private set; }
    public float EnemyFlagActionMinIntervalSec { get; private set; }
}
