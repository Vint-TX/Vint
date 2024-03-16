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

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) { // todo send to moderators in discord or smth else
        using DbConnection db = new();
        
        Player? targetPlayer = db.Players.SingleOrDefault(player => player.Id == UserId);

        if (targetPlayer == null) return;
        
        //Oh
        connection.Server.DiscordBot?.SendReport($"{connection.Player.Username} reported {targetPlayer.Username}");

        Relation? relation = db.Relations
            .SingleOrDefault(relation => relation.SourcePlayerId == SourceId &&
                                         relation.TargetPlayerId == UserId);

        if (relation == null) {
            db.Insert(new Relation { SourcePlayer = connection.Player, TargetPlayer = targetPlayer, Types = RelationTypes.Reported });
        } else {
            relation.Types |= RelationTypes.Reported;
            db.Update(relation);
        }
    }
}