using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450264928332)]
public class BreakOffFriendEvent : FriendBaseEvent, IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();
        Player? player = db.Players.SingleOrDefault(player => player.Id == User.Id);

        if (player == null) return;

        db.BeginTransaction();
        db.Relations
            .Where(relation => relation.SourcePlayerId == player.Id &&
                               relation.TargetPlayerId == connection.Player.Id)
            .Set(relation => relation.Types,
                relation => relation.Types & ~RelationTypes.Friend)
            .Update();

        db.Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id &&
                               relation.TargetPlayerId == player.Id)
            .Set(relation => relation.Types,
                relation => relation.Types & ~RelationTypes.Friend)
            .Update();

        db.CommitTransaction();
        connection.Send(new AcceptedFriendRemovedEvent(player.Id), connection.User);
        connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == player.Id)?
            .Send(new AcceptedFriendRemovedEvent(connection.Player.Id), User);
    }
}