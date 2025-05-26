using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Events;

[ProtocolId(635906273125139964)]
public class CheckEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;
    public bool IncludeUnconfirmed { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Email = Email.Trim();
        EmailValidationResult result = await EmailUtils.Validate(Email, IncludeUnconfirmed);

        switch (result) {
            case EmailValidationResult.Vacant:
                await connection.Send(new EmailVacantEvent(Email));
                break;

            case EmailValidationResult.Occupied:
                await connection.Send(new EmailOccupiedEvent(Email));
                break;

            case EmailValidationResult.Invalid:
                await connection.Send(new EmailInvalidEvent(Email));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
