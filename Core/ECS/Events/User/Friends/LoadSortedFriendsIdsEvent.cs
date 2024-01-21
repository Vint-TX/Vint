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

        var relations = db.Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id)
            .LoadWith(relation => relation.TargetPlayer)
            .Select(relation => new { Id = relation.TargetPlayerId, relation.TargetPlayer.Username, relation.Types })
            .ToArray()
            .Select(relation => new { 
                relation.Id,
                relation.Username, 
                RelationTypes = relation.Types, 
                IsOnline = connections.Where(conn => conn.IsOnline).Any(conn => conn.Player.Id == relation.Id), 
                InLobby = connections.Where(conn => conn is { IsOnline: true, InLobby: true }).Any(conn => conn.Player.Id == relation.Id)
            })
            .OrderByDescending(player => player.IsOnline)
            .ThenByDescending(player => player.InLobby)
            .ThenBy(player => player.Username)
            .ToArray();

        Dictionary<long, string> friends = relations
            .Where(player => (player.RelationTypes & RelationTypes.Friend) == RelationTypes.Friend)
            .ToDictionary(player => player.Id, player => player.Username);

        Dictionary<long, string> incoming = relations
            .Where(player => (player.RelationTypes & RelationTypes.IncomingRequest) == RelationTypes.IncomingRequest)
            .ToDictionary(player => player.Id, player => player.Username);

        Dictionary<long, string> outgoing = relations
            .Where(player => (player.RelationTypes & RelationTypes.OutgoingRequest) == RelationTypes.OutgoingRequest)
            .ToDictionary(player => player.Id, player => player.Username);

        connection.Send(new SortedFriendsIdsLoadedEvent(friends, incoming, outgoing));
    }
}