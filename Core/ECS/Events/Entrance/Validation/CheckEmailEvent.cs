using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273125139964)]
public class CheckEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;
    public bool IncludeUnconfirmed { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        switch (Email[..Email.IndexOf('@')]) {
            case "taken":
                connection.Send(new EmailOccupiedEvent(Email));
                break;

            case "invalid":
                connection.Send(new EmailInvalidEvent(Email));
                break;

            default:
                connection.Send(new EmailVacantEvent(Email));
                break;
        }
    }
}