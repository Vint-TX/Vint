using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1506939739582)]
public class ReportUserByUserIdEvent(
    DiscordBot? discordBot
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
            .SingleOrDefaultAsync();

        if (reportedUsername == null) return;

        if (discordBot != null)
            await discordBot.SendReport($"{reportedUsername} has been reported", connection.Player.Username);

        Report report = new() {
            ReporterId = connection.UserContainer.Id,
            ReportedId = UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            InteractionSource = InteractionSource
        };

        await db.InsertAsync(report);
    }
}
