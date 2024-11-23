using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Fraction;

[ProtocolId(1544956558339)]
public class RestrictionByUserFractionComponent : IComponent {
    public long FractionId { get; set; }
}
