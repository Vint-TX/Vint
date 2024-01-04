using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Leagues;

[ProtocolId(1505728594733)]
public class SeasonEndDateComponent(
    DateTimeOffset? endDate = null
) : IComponent {
    public DateTimeOffset? EndDate { get; private set; } = endDate ?? DateTimeOffset.UtcNow.AddMonths(1);
}