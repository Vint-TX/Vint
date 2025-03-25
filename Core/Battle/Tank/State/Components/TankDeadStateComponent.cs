using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.State.Components;

[ProtocolId(-2656312914607478436)]
public class TankDeadStateComponent : IComponent {
    public DateTimeOffset EndTime { get; private set; } = DateTimeOffset.UtcNow.AddSeconds(3);
}
