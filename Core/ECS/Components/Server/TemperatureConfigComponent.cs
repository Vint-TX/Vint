namespace Vint.Core.ECS.Components.Server;

public class TemperatureConfigComponent : IComponent {
    public float MaxTemperature { get; private set; }
    public float MinTemperature { get; private set; }
    public float AutoIncrementInMs { get; private set; }
    public float AutoDecrementInMs { get; private set; }
    public float TactPeriodInMs { get; private set; }
}