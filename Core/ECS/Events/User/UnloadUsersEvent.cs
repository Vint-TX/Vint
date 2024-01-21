using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555309592)]
public class UnloadUsersEvent : IServerEvent {
    public HashSet<long> UserIds { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.InLobby)
            UserIds.ExceptWith(connection.BattlePlayer!.Battle.Players.Select(battlePlayer => battlePlayer.PlayerConnection.User.Id));
        
        IEnumerable<IEntity> users = UserIds
            .Select(id => connection.SharedEntities.SingleOrDefault(entity => entity.Id == id) ??
                          EntityRegistry.GetOrDefault(id)!)
            .Where(user => user != null!)
            .ToList();
        
        connection.Unshare(users);
    }
}