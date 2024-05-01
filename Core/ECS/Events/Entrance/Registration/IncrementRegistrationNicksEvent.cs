using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Registration;

[ProtocolId(1453881244963)]
public class IncrementRegistrationNicksEvent : IServerEvent { // TODO statistics?
    [ProtocolName("Nick")] public string Nickname { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) => Task.CompletedTask;
}
