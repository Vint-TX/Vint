using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Garage.Events;

[ProtocolId(1479879892222)]
public class ClientGarageFirstLoadEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) => Task.CompletedTask;
}
