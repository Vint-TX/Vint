namespace Vint.Core.ECS.Components.Server;

public class TemperatureConfigComponent : IComponent {
    public float MaxTemperature { get; set; }
    public float MinTemperature { get; set; }
    public float AutoIncrementInMs { get; set; }
    public float AutoDecrementInMs { get; set; }
    public float TactPeriodInMs { get; set; }
}