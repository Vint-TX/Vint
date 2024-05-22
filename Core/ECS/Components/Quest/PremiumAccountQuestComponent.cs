using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Quest;

[ProtocolId(1513252653655)]
public class PremiumAccountQuestComponent(
    DateTimeOffset endDate
) : IComponent {
    public DateTimeOffset EndDate { get; private set; } = endDate;
}
