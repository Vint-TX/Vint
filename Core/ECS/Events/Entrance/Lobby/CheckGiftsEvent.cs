using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Lobby;

[ProtocolId(636446543585160318)]
public class CheckGiftsEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        // TODO
        return Task.CompletedTask;
    }
}
