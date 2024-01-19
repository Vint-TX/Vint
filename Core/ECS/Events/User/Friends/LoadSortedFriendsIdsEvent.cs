using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450243543232)]
public class LoadSortedFriendsIdsEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();
        IPlayerConnection[] connections = connection.Server.PlayerConnections.ToArray();

        Relation[] relations = db.Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id)
            .LoadWith(relation => relation.TargetPlayer)
            .ToArray()
            .OrderBy(relation => connections.SingleOrDefault(conn => conn.Player.Id == relation.TargetPlayerId) != null)
            .ThenBy(relation => connections.SingleOrDefault(conn => conn.Player.Id == relation.TargetPlayerId)?.IsOnline)
            .ThenBy(relation => connections.SingleOrDefault(conn => conn.Player.Id == relation.TargetPlayerId)?.InLobby)
            .ThenBy(relation => relation.TargetPlayer.Username)
            .ToArray();

        Dictionary<long, string> friends = relations
            .Where(relation => (relation.Types & RelationTypes.Friend) == RelationTypes.Friend)
            .ToDictionary(relation => relation.TargetPlayerId,
                relation => relation.TargetPlayer.Username);

        Dictionary<long, string> incoming = relations
            .Where(relation => (relation.Types & RelationTypes.IncomingRequest) == RelationTypes.IncomingRequest)
            .ToDictionary(relation => relation.TargetPlayerId,
                relation => relation.TargetPlayer.Username);

        Dictionary<long, string> outgoing = relations
            .Where(relation => (relation.Types & RelationTypes.OutgoingRequest) == RelationTypes.OutgoingRequest)
            .ToDictionary(relation => relation.TargetPlayerId,
                relation => relation.TargetPlayer.Username);

        connection.Send(new SortedFriendsIdsLoadedEvent(friends, incoming, outgoing));
    }
}