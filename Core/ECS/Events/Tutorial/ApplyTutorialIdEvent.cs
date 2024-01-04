using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Tutorial;

[ProtocolId(1505212007257)]
public class ApplyTutorialIdEvent : IServerEvent {
    public long Id { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        // todo
    }
}