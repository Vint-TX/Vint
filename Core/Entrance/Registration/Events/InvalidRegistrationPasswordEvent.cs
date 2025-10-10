using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Registration.Events;

[ProtocolId(1453881282573)]
public class InvalidRegistrationPasswordEvent : IServerEvent { // TODO statistics?
    public Task Execute(IPlayerConnection connection, IEntity[] entities) => Task.CompletedTask;
}
