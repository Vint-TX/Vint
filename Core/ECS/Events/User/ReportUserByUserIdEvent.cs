using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1506939739582)]
public class ReportUserByUserIdEvent : IServerEvent {
    public InteractionSource InteractionSource { get; set; }
    public long SourceId { get; set; }
    public long UserId { get; set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) { // todo improve
        await using DbConnection db = new();

        Player? targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == UserId);

        if (targetPlayer == null) return;

        if (connection.Server.DiscordBot != null)
            await connection.Server.DiscordBot.SendReport($"{targetPlayer.Username} has been reported", connection.Player.Username);

        Relation? relation = await db.Relations
            .SingleOrDefaultAsync(relation => relation.SourcePlayerId == SourceId &&
                                         relation.TargetPlayerId == UserId);

        if (relation == null) {
            await db.InsertAsync(new Relation { SourcePlayer = connection.Player, TargetPlayer = targetPlayer, Types = RelationTypes.Reported });
        } else {
            relation.Types |= RelationTypes.Reported;
            await db.UpdateAsync(relation);
        }
    }
}
