using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1498740539984)]
public class LoadSortedFriendsIdsWithNicknamesEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();
        IPlayerConnection[] connections = connection.Server.PlayerConnections.ToArray();

        Dictionary<long, string> friends = db.Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id)
            .LoadWith(relation => relation.TargetPlayer)
            .ToArray()
            .OrderBy(relation => connections.SingleOrDefault(conn => conn.Player.Id == relation.TargetPlayerId) != null)
            .ThenBy(relation => connections.First(conn => conn.Player.Id == relation.TargetPlayerId).IsOnline)
            .ThenBy(relation => connections.First(conn => conn.Player.Id == relation.TargetPlayerId).InLobby)
            .ThenBy(relation => relation.TargetPlayer.Username)
            .ToDictionary(relation => relation.TargetPlayerId, relation => relation.TargetPlayer.Username);

        connection.Send(new SortedFriendsIdsWithNicknamesLoadedEvent(friends));
    }
}