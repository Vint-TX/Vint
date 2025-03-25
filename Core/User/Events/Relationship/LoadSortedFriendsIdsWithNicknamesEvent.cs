using System.Collections.Frozen;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1498740539984)]
public class LoadSortedFriendsIdsWithNicknamesEvent(
    GameServer server
) : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        FrozenDictionary<long, string> friendsUnsorted;

        await using (DbConnection db = new()) {
            friendsUnsorted = (await db.Friends
                .Where(friend => friend.UserId == connection.UserContainer.Id)
                .LoadWith(friend => friend.FriendPlayer)
                .ToDictionaryAsync(friend => friend.FriendId, friend => friend.FriendPlayer.Username))
                .ToFrozenDictionary();
        }

        FrozenDictionary<long, PlayerStatus> statuses = server.PlayerConnections.Values
            .Where(conn => conn.IsLoggedIn && friendsUnsorted.ContainsKey(conn.UserContainer.Id))
            .ToFrozenDictionary(
                conn => conn.UserContainer.Id,
                conn => conn.InLobby ? PlayerStatus.InLobby : PlayerStatus.Online);

        await connection.Send(new SortedFriendsIdsWithNicknamesLoadedEvent(friendsUnsorted
            .Order(new FriendComparer(id => statuses.GetValueOrDefault(id, PlayerStatus.Offline)))
            .ToDictionary()));
    }
}

file enum PlayerStatus {
    Offline,
    Online,
    InLobby
}

file class FriendComparer(
    Func<long, PlayerStatus> getStatus
) : IComparer<KeyValuePair<long, string>> {
    public int Compare(KeyValuePair<long, string> x, KeyValuePair<long, string> y) {
        int statusComparison = getStatus(y.Key).CompareTo(getStatus(x.Key));
        return statusComparison != 0
            ? statusComparison
            : string.Compare(x.Value, y.Value, StringComparison.OrdinalIgnoreCase);
    }
}
