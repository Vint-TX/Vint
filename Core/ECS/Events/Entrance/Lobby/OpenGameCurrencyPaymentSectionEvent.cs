using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Lobby;

[ProtocolId(1455283639698)]
public class OpenGameCurrencyPaymentSectionEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        // TODO
        return Task.CompletedTask;
    }
}
