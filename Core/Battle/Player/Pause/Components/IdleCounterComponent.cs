using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Pause.Components;

[ProtocolId(2930474294118078222)]
public class IdleCounterComponent(
    long skippedMillis,
    DateTimeOffset? skipBeginDate = null
) : IComponent {
    public long SkippedMillis { get; set; } = skippedMillis;
    public DateTimeOffset? SkipBeginDate { get; set; } = skipBeginDate;
}
