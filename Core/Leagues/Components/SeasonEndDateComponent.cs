using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Leagues.Components;

[ProtocolId(1505728594733)]
public class SeasonEndDateComponent(
    DateTimeOffset? endDate = null
) : IComponent {
    public DateTimeOffset? EndDate { get; private set; } = endDate ?? DateTimeOffset.UtcNow.AddMonths(1);
}
