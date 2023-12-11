using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Registration;

[ProtocolId(1453881244963)]
public class IncrementRegistrationNicksEvent : IServerEvent {
    [ProtocolName("nick")] public string Nickname { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        // TODO statistics?
    }
}
