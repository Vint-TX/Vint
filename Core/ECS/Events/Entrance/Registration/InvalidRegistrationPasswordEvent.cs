using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Registration;

[ProtocolId(1453881282573)]
public class InvalidRegistrationPasswordEvent : IServerEvent { // TODO statistics?
    public Task Execute(IPlayerConnection connection, IEntity[] entities) => Task.CompletedTask;
}
