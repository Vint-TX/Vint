using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1502716170372)]
public class UserReputationComponent(
    double reputation
) : IComponent {
    public double Reputation { get; set; } = reputation;
}