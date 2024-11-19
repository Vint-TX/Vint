using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Limit;

[ProtocolId(-3596341255560623830)]
public class TimeLimitComponent(
    TimeSpan timeLimit,
    TimeSpan warmingUpTimeLimit
) : IComponent {
    [ProtocolTimeKind<long>(TimeSpanKind.Seconds)]
    [ProtocolName("TimeLimitSec")]
    public TimeSpan TimeLimit { get; } = timeLimit;

    [ProtocolTimeKind<long>(TimeSpanKind.Seconds)]
    [ProtocolName("WarmingUpTimeLimitSec")]
    public TimeSpan WarmingUpTimeLimit { get; } = warmingUpTimeLimit;
}
