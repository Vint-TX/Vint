using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1454623211245)]
public class UserInteractionDataRequestEvent : IServerEvent {
    public long UserId { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();
        Player? player = db.Players.SingleOrDefault(player => player.Id == UserId);

        if (player == null) return;

        Relation? relation = db.Relations
            .SingleOrDefault(relation => relation.SourcePlayerId == connection.Player.Id &&
                                         relation.TargetPlayerId == player.Id);

        bool noRelations = !IsFriend(relation) &&
                           !IsBlocked(relation) &&
                           !IsIncoming(relation) &&
                           !IsOutgoing(relation);

        bool canRequestFriend = connection.Player.Id != player.Id && noRelations;
        connection.Send(new UserInteractionDataResponseEvent(UserId,
            player.Username,
            canRequestFriend,
            !noRelations && IsOutgoing(relation),
            !noRelations && IsBlocked(relation),
            IsReported(relation)));
        return;

        static bool IsFriend(Relation? relation) => (relation?.Types & RelationTypes.Friend) == RelationTypes.Friend;
        static bool IsBlocked(Relation? relation) => (relation?.Types & RelationTypes.Blocked) == RelationTypes.Blocked;
        static bool IsReported(Relation? relation) => (relation?.Types & RelationTypes.Reported) == RelationTypes.Reported;
        static bool IsIncoming(Relation? relation) => (relation?.Types & RelationTypes.IncomingRequest) == RelationTypes.IncomingRequest;
        static bool IsOutgoing(Relation? relation) => (relation?.Types & RelationTypes.OutgoingRequest) == RelationTypes.OutgoingRequest;
    }
}