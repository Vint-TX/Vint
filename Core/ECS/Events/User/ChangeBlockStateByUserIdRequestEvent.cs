using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1507198221820)]
public class ChangeBlockStateByUserIdRequestEvent : IServerEvent {
    public InteractionSource InteractionSource { get; set; }
    public long SourceId { get; set; }
    public long UserId { get; set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await using DbConnection db = new();
        Player? targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == UserId);

        if (targetPlayer == null) return;

        Relation? thisToTargetRelation =
            await db.Relations.SingleOrDefaultAsync(relation => relation.SourcePlayerId == connection.UserContainer.Id &&
                                                                relation.TargetPlayerId == UserId);

        await db.BeginTransactionAsync();

        if (thisToTargetRelation == null) { // no relation to target; block
            thisToTargetRelation = new Relation { SourcePlayer = connection.Player, TargetPlayer = targetPlayer, Types = RelationTypes.Blocked };
            await db.InsertAsync(thisToTargetRelation);
        } else { // change the state of relations
            if ((thisToTargetRelation.Types & RelationTypes.Blocked) == RelationTypes.Blocked) { // player already blocked target player; unblock
                thisToTargetRelation.Types &= ~(RelationTypes.Blocked |
                                                RelationTypes.Friend |
                                                RelationTypes.IncomingRequest |
                                                RelationTypes.OutgoingRequest);
            } else { // target player is not blocked; block
                thisToTargetRelation.Types |= RelationTypes.Blocked;

                Relation? targetToThisRelation = await db.Relations.SingleOrDefaultAsync(relation => relation.SourcePlayerId == UserId &&
                    relation.TargetPlayerId == connection.UserContainer.Id);

                if (targetToThisRelation != null) {
                    targetToThisRelation.Types &= ~(RelationTypes.Friend | RelationTypes.IncomingRequest | RelationTypes.OutgoingRequest);

                    await db.UpdateAsync(targetToThisRelation);
                }
            }

            await db.UpdateAsync(thisToTargetRelation);
        }

        await db.CommitTransactionAsync();
    }
}
