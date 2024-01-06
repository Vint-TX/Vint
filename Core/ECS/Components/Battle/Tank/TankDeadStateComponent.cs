using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Tank;

[ProtocolId(-2656312914607478436)]
public class TankDeadStateComponent : IComponent {
    public DateTimeOffset EndTime { get; private set; } = DateTimeOffset.UtcNow.AddSeconds(3);
}