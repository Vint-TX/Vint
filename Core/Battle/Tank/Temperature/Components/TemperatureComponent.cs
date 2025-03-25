using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Tank;

[ProtocolId(6673681254298647708)]
public class TemperatureComponent : IComponent {
    public float Temperature { get; set; } = 0;
}
