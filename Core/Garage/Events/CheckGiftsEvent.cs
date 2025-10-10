using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Garage.Events;

[ProtocolId(636446543585160318)]
public class CheckGiftsEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        // TODO
        Task.CompletedTask;
}
