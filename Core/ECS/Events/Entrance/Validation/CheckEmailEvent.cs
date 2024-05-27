using System.Net.Mail;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273125139964)]
public class CheckEmailEvent : IServerEvent {
    public string Email { get; private set; } = null!;
    public bool IncludeUnconfirmed { get; private set; } //todo

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        try {
            MailAddress email = new(Email);

            await using DbConnection db = new();

            if (await db.Players.AnyAsync(player => player.Email == email.Address))
                await connection.Send(new EmailOccupiedEvent(Email));
            else await connection.Send(new EmailVacantEvent(Email));
        } catch {
            await connection.Send(new EmailInvalidEvent(Email));
        }
    }
}
