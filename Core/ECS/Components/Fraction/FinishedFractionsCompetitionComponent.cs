using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Fraction;

[ProtocolId(1545394828752)]
public class FinishedFractionsCompetitionComponent(
    long winnerId
) : IComponent {
    public long WinnerId { get; private set; } = winnerId;
}