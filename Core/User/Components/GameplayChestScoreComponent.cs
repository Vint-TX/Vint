using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(636389758870600269)]
public class GameplayChestScoreComponent(
    long current,
    long limit = 1000
) : PrivateComponent {
    public long Current { get; set; } = current;
    public long Limit { get; private set; } = limit;
}
