using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Pause;

[ProtocolId(2930474294118078222)]
public class IdleCounterComponent(
    long skippedMillis,
    DateTimeOffset? skipBeginDate = null
) : IComponent {
    public long SkippedMillis { get; set; } = skippedMillis;
    public DateTimeOffset? SkipBeginDate { get; set; } = skipBeginDate;
}