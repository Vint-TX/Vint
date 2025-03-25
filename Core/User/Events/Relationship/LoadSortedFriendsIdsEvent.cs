using System.Collections.Frozen;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450243543232)]
public class LoadSortedFriendsIdsEvent(
    GameServer server
) : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long userId = connection.UserContainer.Id;
        FrozenSet<Relation> relationsUnsorted;

        await using (DbConnection db = new()) {
            List<Relation> friendsUnsorted = await db.Friends
                .Where(friend => friend.UserId == userId)
                .LoadWith(friend => friend.FriendPlayer)
                .Select(friend => new Relation(friend.FriendId, friend.FriendPlayer.Username, RelationType.Friend))
                .ToListAsync();

            List<Relation> outgoingUnsorted = await db.FriendRequests
                .Where(request => request.SenderId == userId)
                .LoadWith(request => request.FriendPlayer)
                .Select(request => new Relation(request.FriendId, request.FriendPlayer.Username, RelationType.Outgoing))
                .ToListAsync();

            List<Relation> incomingUnsorted = await db.FriendRequests
                .Where(request => request.FriendId == userId)
                .LoadWith(request => request.SenderPlayer)
                .Select(request => new Relation(request.SenderId, request.SenderPlayer.Username, RelationType.Incoming))
                .ToListAsync();

            relationsUnsorted = [..friendsUnsorted, ..outgoingUnsorted, ..incomingUnsorted];
        }

        FrozenSet<long> ids = relationsUnsorted.Select(relation => relation.UserId).ToFrozenSet();
        FrozenDictionary<long, PlayerStatus> statuses = server.PlayerConnections.Values
            .Where(conn => conn.IsLoggedIn && ids.Contains(conn.UserContainer.Id))
            .ToFrozenDictionary(
                conn => conn.UserContainer.Id,
                conn => conn.InLobby ? PlayerStatus.InLobby : PlayerStatus.Online);

        Dictionary<long, string> friends = [];
        Dictionary<long, string> outgoing = [];
        Dictionary<long, string> incoming = [];

        foreach (Relation relation in relationsUnsorted.Order(new RelationComparer(id => statuses.GetValueOrDefault(id, PlayerStatus.Offline)))) {
            switch (relation.Type) {
                case RelationType.Friend:
                    friends[relation.UserId] = relation.Username;
                    break;

                case RelationType.Outgoing:
                    outgoing[relation.UserId] = relation.Username;
                    break;

                case RelationType.Incoming:
                    incoming[relation.UserId] = relation.Username;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(null);
            }
        }

        await connection.Send(new SortedFriendsIdsLoadedEvent(friends, incoming, outgoing));
    }
}

readonly file struct Relation(
    long userId,
    string username,
    RelationType type
) : IEquatable<Relation> {
    public long UserId { get; } = userId;
    public string Username { get; } = username;
    public RelationType Type { get; } = type;

    public bool Equals(Relation other) => UserId == other.UserId;

    public override bool Equals(object? obj) => obj is Relation other && Equals(other);

    public override int GetHashCode() => UserId.GetHashCode();
}

file enum PlayerStatus {
    Offline,
    Online,
    InLobby
}

file enum RelationType {
    Friend,
    Outgoing,
    Incoming
}

file class RelationComparer(
    Func<long, PlayerStatus> getStatus
) : IComparer<Relation> {
    public int Compare(Relation x, Relation y) {
        int statusComparison = getStatus(y.UserId).CompareTo(getStatus(x.UserId));
        return statusComparison != 0
            ? statusComparison
            : string.Compare(x.Username, y.Username, StringComparison.OrdinalIgnoreCase);
    }
}
