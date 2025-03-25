using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1502716170372)]
public class UserReputationComponent(
    double reputation
) : IComponent {
    public double Reputation { get; set; } = reputation;
}
