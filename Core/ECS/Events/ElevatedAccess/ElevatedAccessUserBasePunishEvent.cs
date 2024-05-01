using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.ElevatedAccess;

public abstract class ElevatedAccessUserBasePunishEvent : IServerEvent {
    [ProtocolName("Uid")] public string Username { get; protected set; } = null!;
    public string Reason { get; protected set; } = null!;

    public abstract Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities);
}
