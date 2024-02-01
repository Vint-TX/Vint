using Vint.Core.Database;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1499174753575)]
public class UserStatisticsComponent : IComponent {
    public UserStatisticsComponent(long playerId) {
        using DbConnection db = new();

        Statistics = db.Statistics.Single(stats => stats.PlayerId == playerId).CollectClientSide();
    }

    public Dictionary<string, long> Statistics { get; private set; }
}