using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Entrance.Registration;

[ProtocolId(1453881282573)]
public class InvalidRegistrationPasswordEvent : IServerEvent { // TODO statistics?
    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) => Task.CompletedTask;
}
