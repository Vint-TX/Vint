using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1469526368502)]
public class SearchUserIdByUidEvent : IServerEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.Player.Username == Username) return;

        await using DbConnection db = new();
        long id = await db.Players
            .Where(player => player.Username == Username)
            .Select(player => player.Id)
            .SingleOrDefaultAsync();

        bool found = id != default;
        await connection.Send(new SearchUserIdByUidResultEvent(found, id));
    }
}
