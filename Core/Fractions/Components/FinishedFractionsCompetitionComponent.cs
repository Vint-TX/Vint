using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Fractions.Components;

[ProtocolId(1545394828752)]
public class FinishedFractionsCompetitionComponent(
    long winnerId
) : IComponent {
    public long WinnerId { get; private set; } = winnerId;
}
