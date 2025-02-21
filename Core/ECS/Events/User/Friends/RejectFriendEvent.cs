using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450168274692)]
public class RejectFriendEvent(
    GameServer server
) : FriendBaseEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await using DbConnection db = new();
        Player? player = await db.Players.SingleOrDefaultAsync(player => player.Id == User);

        if (player == null) return;

        await db.BeginTransactionAsync();

        await db
            .Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id && relation.TargetPlayerId == player.Id)
            .Set(relation => relation.Types, relation => relation.Types & ~(RelationTypes.Friend | RelationTypes.IncomingRequest))
            .UpdateAsync();

        await db
            .Relations
            .Where(relation => relation.SourcePlayerId == player.Id && relation.TargetPlayerId == connection.Player.Id)
            .Set(relation => relation.Types, relation => relation.Types & ~(RelationTypes.Friend | RelationTypes.OutgoingRequest))
            .UpdateAsync();

        await db.CommitTransactionAsync();
        await connection.Send(new IncomingFriendRemovedEvent(player.Id), connection.UserContainer.Entity);

        IPlayerConnection? targetConnection = server
            .PlayerConnections
            .Values
            .Where(conn => conn.IsLoggedIn)
            .SingleOrDefault(conn => conn.Player.Id == player.Id);

        if (targetConnection != null)
            await targetConnection.Send(new OutgoingFriendRemovedEvent(connection.Player.Id), UserContainer.Entity);
    }
}
