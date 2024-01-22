using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.ElevatedAccess;

public abstract class ElevatedAccessUserBasePunishEvent : IServerEvent {
    [ProtocolIgnore] protected static IEntity GlobalChat { get; } = GlobalEntities.GetEntity("chats", "En");
    
    [ProtocolName("Uid")] public string Username { get; protected set; } = null!;
    public string Reason { get; protected set; } = null!;
    
    public abstract void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities);
}