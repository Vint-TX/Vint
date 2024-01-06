using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Tank;

[ProtocolId(6673681254298647708)]
public class TemperatureComponent(
    float temperature
) : IComponent {
    public float Temperature { get; set; } = temperature;
}