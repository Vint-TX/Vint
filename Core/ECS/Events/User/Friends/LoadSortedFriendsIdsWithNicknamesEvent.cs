using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1498740539984)]
public class LoadSortedFriendsIdsWithNicknamesEvent(
    GameServer server
) : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await using DbConnection db = new();
        List<IPlayerConnection> connections = server.PlayerConnections.Values.ToList();

        Dictionary<long, string> friends = db
            .Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id)
            .LoadWith(relation => relation.TargetPlayer)
            .Select(relation => new { Id = relation.TargetPlayerId, relation.TargetPlayer.Username })
            .ToList()
            .Select(relation => new {
                relation.Id,
                relation.Username,
                IsOnline = connections
                    .Where(conn => conn.IsOnline)
                    .Any(conn => conn.Player.Id == relation.Id),
                InLobby = connections
                    .Where(conn => conn is { IsOnline: true, InLobby: true })
                    .Any(conn => conn.Player.Id == relation.Id)
            })
            .OrderByDescending(player => player.IsOnline)
            .ThenBy(player => player.InLobby)
            .ThenBy(player => player.Username)
            .ToDictionary(relation => relation.Id, relation => relation.Username);

        await connection.Send(new SortedFriendsIdsWithNicknamesLoadedEvent(friends));
    }
}
