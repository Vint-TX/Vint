using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Autopilot.Components;

[ProtocolId(1450950140134)]
public class TankAutopilotComponent(
    int id
) : IComponent {
    public IEntity? Session { get; set; }
    [ProtocolName("Version")] public int Id { get; } = id;
}
