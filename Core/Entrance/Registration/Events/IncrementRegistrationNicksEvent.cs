using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Registration.Events;

[ProtocolId(1453881244963)]
public class IncrementRegistrationNicksEvent : IServerEvent { // TODO statistics?
    [ProtocolName("Nick")] public string Nickname { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEntity[] entities) => Task.CompletedTask;
}
