using Vint.Core.Database.Models;

namespace Vint.Core.Utils;

public static class RelationUtils {
    public static bool IsFriend(this Relation relation) => (relation.Types & RelationTypes.Friend) == RelationTypes.Friend;

    public static bool IsIncoming(this Relation relation) => (relation.Types & RelationTypes.IncomingRequest) == RelationTypes.IncomingRequest;

    public static bool IsOutgoing(this Relation relation) => (relation.Types & RelationTypes.OutgoingRequest) == RelationTypes.OutgoingRequest;

    public static bool IsBlocked(this Relation relation) => (relation.Types & RelationTypes.Blocked) == RelationTypes.Blocked;

    public static bool IsReported(this Relation relation) => (relation.Types & RelationTypes.Reported) == RelationTypes.Reported;
}