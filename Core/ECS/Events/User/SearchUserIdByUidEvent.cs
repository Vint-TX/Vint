using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1469526368502)]
public class SearchUserIdByUidEvent : IServerEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        await using DbConnection db = new();

        Player searcherPlayer = connection.Player;
        Player? searchedPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == Username);

        if (searchedPlayer == null) {
            await connection.Send(new SearchUserIdByUidResultEvent(false, 0));
            return;
        }

        bool noRelations = await db.Relations
            .Where(relation => relation.SourcePlayerId == searchedPlayer.Id &&
                               relation.TargetPlayerId == searcherPlayer.Id)
            .AllAsync(relation => (relation.Types & RelationTypes.Friend) != RelationTypes.Friend &&
                             (relation.Types & RelationTypes.Blocked) != RelationTypes.Blocked &&
                             (relation.Types & RelationTypes.IncomingRequest) != RelationTypes.IncomingRequest &&
                             (relation.Types & RelationTypes.OutgoingRequest) != RelationTypes.OutgoingRequest);

        bool canRequestFriend = searcherPlayer.Id != searchedPlayer.Id && noRelations;

        await connection.Send(new SearchUserIdByUidResultEvent(canRequestFriend, searchedPlayer.Id));
    }
}
