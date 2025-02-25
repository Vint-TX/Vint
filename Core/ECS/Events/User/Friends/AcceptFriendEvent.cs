using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450168255217)]
public class AcceptFriendEvent(
    GameServer server
) : FriendBaseEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await using DbConnection db = new();
        Player? player = await db.Players.SingleOrDefaultAsync(player => player.Id == User);

        if (player == null) return;

        await db.BeginTransactionAsync();

        await db
            .Relations
            .Where(relation => relation.SourcePlayerId == player.Id && relation.TargetPlayerId == connection.Player.Id)
            .Set(relation => relation.Types, relation => relation.Types & ~RelationTypes.OutgoingRequest | RelationTypes.Friend)
            .UpdateAsync();

        await db
            .Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id && relation.TargetPlayerId == player.Id)
            .Set(relation => relation.Types, relation => relation.Types & ~RelationTypes.IncomingRequest | RelationTypes.Friend)
            .UpdateAsync();

        await db.CommitTransactionAsync();
        await connection.Send(new IncomingFriendRemovedEvent(player.Id), connection.UserContainer.Entity);
        await connection.Send(new AcceptedFriendAddedEvent(player.Id), connection.UserContainer.Entity);

        IPlayerConnection? playerConnection = server
            .PlayerConnections
            .Values
            .Where(conn => conn.IsLoggedIn)
            .SingleOrDefault(conn => conn.Player.Id == player.Id);

        if (playerConnection == null) return;

        await playerConnection.Send(new OutgoingFriendRemovedEvent(connection.Player.Id), UserContainer.Entity);
        await playerConnection.Send(new AcceptedFriendAddedEvent(connection.Player.Id), UserContainer.Entity);
    }
}
