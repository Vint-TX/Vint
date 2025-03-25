using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class TemperatureConfigComponent : IComponent {
    public float MaxTemperature { get; private set; }
    public float MinTemperature { get; private set; }
    public float AutoIncrementInMs { get; set; }
    public float AutoDecrementInMs { get; set; }
    public float TactPeriodInMs { get; private set; }
}
