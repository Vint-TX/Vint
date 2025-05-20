using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1506939739582)]
public class ReportUserByUserIdEvent(
    ApiServer apiServer
) : IServerEvent {
    public InteractionSource InteractionSource { get; set; }
    public long SourceId { get; set; }
    public long UserId { get; set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.UserContainer.Id == UserId) return;

        await using DbConnection db = new();

        string? reportedUsername = await db.Players
            .Where(player => player.Id == UserId)
            .Select(player => player.Username)
            .FirstOrDefaultAsync();

        if (reportedUsername == null) return;

        await apiServer.Report($"{reportedUsername} has been reported", connection.Player.Username);

        Report report = new() {
            ReporterId = connection.UserContainer.Id,
            ReportedId = UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            InteractionSource = InteractionSource
        };

        await db.InsertAsync(report);
    }
}
