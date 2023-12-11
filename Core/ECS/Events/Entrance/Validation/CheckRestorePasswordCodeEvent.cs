using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1460402752765)]
public class CheckRestorePasswordCodeEvent : IServerEvent {
    public string Code { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (Code == "valid") connection.Send(new RestorePasswordCodeValidEvent(Code));
        else connection.Send(new RestorePasswordCodeInvalidEvent(Code));
    }
}
