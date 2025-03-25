using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Mode.Team.Components;

public class CtfConfigComponent : IComponent {
    public float MinDistanceFromMineToBase { get; private set; }
    public float EnemyFlagActionMinIntervalSec { get; private set; }
}
