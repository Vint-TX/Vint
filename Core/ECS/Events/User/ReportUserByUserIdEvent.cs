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

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) { // todo improve
        await using DbConnection db = new();

        Player? targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == UserId);

        if (targetPlayer == null) return;

        if (discordBot != null)
            await discordBot.SendReport($"{targetPlayer.Username} has been reported", connection.Player.Username);

        Relation? relation =
            await db.Relations.SingleOrDefaultAsync(relation => relation.SourcePlayerId == SourceId && relation.TargetPlayerId == UserId);

        if (relation == null) {
            await db.InsertAsync(new Relation { SourcePlayer = connection.Player, TargetPlayer = targetPlayer, Types = RelationTypes.Reported });
        } else {
            relation.Types |= RelationTypes.Reported;
            await db.UpdateAsync(relation);
        }
    }
}
