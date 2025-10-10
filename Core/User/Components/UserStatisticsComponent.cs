using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1499174753575)]
public class UserStatisticsComponent : IComponent {
    public UserStatisticsComponent(long playerId) {
        using DbConnection db = new();

        Statistics? statistics = db.Statistics.FirstOrDefault(stats => stats.PlayerId == playerId);
        Statistics = statistics?.CollectClientSide() ?? [];
    }

    public Dictionary<string, long> Statistics { get; set; }
}
