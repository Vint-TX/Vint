using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Premium.Components;

[ProtocolId(1513252416040)]
public class PremiumAccountBoostComponent(
    DateTimeOffset endDate
) : IComponent {
    public DateTimeOffset EndDate { get; } = endDate;
}
