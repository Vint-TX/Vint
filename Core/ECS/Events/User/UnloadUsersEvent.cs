using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555309592)]
public class UnloadUsersEvent : IServerEvent {
    public HashSet<IEntity> Users { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) => 
        connection.Unshare(Users);
}