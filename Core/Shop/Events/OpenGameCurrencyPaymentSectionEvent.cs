using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Events;

[ProtocolId(1455283639698)]
public class OpenGameCurrencyPaymentSectionEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        // TODO
        Task.CompletedTask;
}
