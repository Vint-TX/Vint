using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1507198221820)]
public class ChangeBlockStateByUserIdRequestEvent : IServerEvent {
    public InteractionSource InteractionSource { get; set; }
    public long SourceId { get; set; }
    public long UserId { get; set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();
        Player? targetPlayer = db.Players.SingleOrDefault(player => player.Id == UserId);

        if (targetPlayer == null) return;

        Relation? thisToTargetRelation = db.Relations
            .SingleOrDefault(relation => relation.SourcePlayerId == SourceId &&
                                         relation.TargetPlayerId == UserId);

        db.BeginTransaction();

        if (thisToTargetRelation == null) {
            thisToTargetRelation = new Relation { SourcePlayer = connection.Player, TargetPlayer = targetPlayer, Types = RelationTypes.Blocked };
            db.Insert(thisToTargetRelation);
        } else {
            if ((thisToTargetRelation.Types & RelationTypes.Blocked) == RelationTypes.Blocked) {
                thisToTargetRelation.Types &= ~(RelationTypes.Blocked |
                                                RelationTypes.Friend |
                                                RelationTypes.IncomingRequest |
                                                RelationTypes.OutgoingRequest);

                Relation? targetToThisRelation = db.Relations.SingleOrDefault(relation => relation.SourcePlayerId == UserId &&
                                                                                          relation.TargetPlayerId == SourceId);

                if (targetToThisRelation != null) {
                    targetToThisRelation.Types &= ~(RelationTypes.Friend |
                                                    RelationTypes.IncomingRequest |
                                                    RelationTypes.OutgoingRequest);

                    db.Update(targetToThisRelation);
                }
            } else {
                thisToTargetRelation.Types |= RelationTypes.Blocked;
            }

            db.Update(thisToTargetRelation);
        }

        db.CommitTransaction();
    }
}