using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Fraction;

[ProtocolId(1544590059379)]
public class FractionUserScoreComponent(
    long earnedPoints
) : IComponent {
    public long EarnedPoints { get; private set; } = earnedPoints;
}