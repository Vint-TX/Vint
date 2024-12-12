using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555309592)]
public class UnloadUsersEvent : IServerEvent {
    public HashSet<IEntity> Users { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        Users.RemoveWhere(user => connection.BattlePlayer?.Battle.Players.Any(battlePlayer => battlePlayer.PlayerConnection.UserContainer.Id == user.Id) ?? false);
        await connection.Unshare(Users);
    }
}
