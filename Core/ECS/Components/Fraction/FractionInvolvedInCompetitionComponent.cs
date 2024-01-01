using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Fraction;

[ProtocolId(1545106623033)]
public class FractionInvolvedInCompetitionComponent(
    long userCount
) : IComponent {
    public long UserCount { get; private set; } = userCount;
}