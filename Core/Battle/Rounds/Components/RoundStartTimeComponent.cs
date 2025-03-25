using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rounds.Components;

[ProtocolId(1436521738148)]
public class RoundStartTimeComponent(
    DateTimeOffset? startTime
) : IComponent {
    public DateTimeOffset? RoundStartTime { get; } = startTime;
}
