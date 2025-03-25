using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Fractions.Components;

[ProtocolId(1545106623033)]
public class FractionInvolvedInCompetitionComponent(
    long userCount
) : IComponent {
    public long UserCount { get; private set; } = userCount;
}
