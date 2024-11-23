using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450264928332)]
public class BreakOffFriendEvent : FriendBaseEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        await using DbConnection db = new();
        Player? player = await db.Players.SingleOrDefaultAsync(player => player.Id == User.Id);

        if (player == null) return;

        await db.BeginTransactionAsync();

        await db
            .Relations
            .Where(relation => relation.SourcePlayerId == player.Id && relation.TargetPlayerId == connection.Player.Id)
            .Set(relation => relation.Types, relation => relation.Types & ~RelationTypes.Friend)
            .UpdateAsync();

        await db
            .Relations
            .Where(relation => relation.SourcePlayerId == connection.Player.Id && relation.TargetPlayerId == player.Id)
            .Set(relation => relation.Types, relation => relation.Types & ~RelationTypes.Friend)
            .UpdateAsync();

        await db.CommitTransactionAsync();
        await connection.Send(new AcceptedFriendRemovedEvent(player.Id), connection.User);

        GameServer server = serviceProvider.GetRequiredService<GameServer>();

        IPlayerConnection? targetConnection = server
            .PlayerConnections
            .Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == player.Id);

        if (targetConnection != null)
            await targetConnection.Send(new AcceptedFriendRemovedEvent(connection.Player.Id), User);
    }
}
