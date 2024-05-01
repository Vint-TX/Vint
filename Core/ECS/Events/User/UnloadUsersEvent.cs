using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555309592)]
public class UnloadUsersEvent : IServerEvent {
    public HashSet<long /*IEntity*/> Users { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) { // bug: client crashes while scrolling friends list
        // todo temporary solution: do not unshare the players

        // Users.RemoveWhere(user => connection.BattlePlayer?.Battle.Players.Any(battlePlayer => battlePlayer.PlayerConnection.User == user) ?? false);

        // connection.Unshare(Users);

        return Task.CompletedTask;
    }
}
