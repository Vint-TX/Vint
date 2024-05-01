using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1515481976775)]
public class ElevatedAccessUserWipeUserItemsEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin || !connection.InLobby) return Task.CompletedTask;

        // todo modules
        return Task.CompletedTask;
    }
}
