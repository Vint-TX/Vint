using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450168255217)]
public class AcceptFriendEvent : FriendBaseEvent, IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();
        Player? player = db.Players.SingleOrDefault(player => player.Id == User.Id);

        if (player == null) return;

        db.BeginTransaction();
        db.Relations
            .Where(relation => relation.SourcePlayerId == player.Id &&
                               relation.TargetPlayerId == connection.Player.Id)
            .Set(relation => relation.Types,
                relation => relation.Types & ~RelationTypes.OutgoingRequest | RelationTypes.Friend)
            .Update();

        db.Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id &&
                               relation.TargetPlayerId == player.Id)
            .Set(relation => relation.Types,
                relation => relation.Types & ~RelationTypes.IncomingRequest | RelationTypes.Friend)
            .Update();

        db.CommitTransaction();
        connection.Send(new IncomingFriendRemovedEvent(player.Id), connection.User);
        connection.Send(new AcceptedFriendAddedEvent(player.Id), connection.User);

        IPlayerConnection? playerConnection = connection.Server.PlayerConnections
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == player.Id);

        if (playerConnection == null) return;

        playerConnection.Send(new OutgoingFriendRemovedEvent(connection.Player.Id), User);
        playerConnection.Send(new AcceptedFriendAddedEvent(connection.Player.Id), User);
    }
}