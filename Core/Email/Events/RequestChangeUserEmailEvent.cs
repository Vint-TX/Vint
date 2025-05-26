using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Events;

[ProtocolId(1457935367814)]
public class RequestChangeUserEmailEvent(
    ApiServer apiServer
) : IServerEvent {
    public string Email { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.IsLoggedIn) return;

        Email = Email.Trim();
        EmailValidationResult validationResult = await EmailUtils.Validate(Email, false);

        switch (validationResult) {
            case EmailValidationResult.Vacant:
                await connection.Send(new EmailVacantEvent(Email));
                break; // break to proceed with email change

            case EmailValidationResult.Occupied:
                await connection.Send(new EmailOccupiedEvent(Email));
                return; // return to prevent further processing

            case EmailValidationResult.Invalid:
                await connection.Send(new EmailInvalidEvent(Email));
                return; // return to prevent further processing

            default:
                throw new ArgumentOutOfRangeException();
        }

        string? oldEmail = connection.Player.Email;
        string newEmail = Email;

        DbConnection db = new();
        EmailConfirmation emailConfirmation = new() {
            PlayerId = connection.Player.Id,
            OldEmail = oldEmail,
            NewEmail = newEmail,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(15)
        };

        await db.BeginTransactionAsync();
        await db.EmailConfirmations
            .Where(ec => ec.PlayerId == emailConfirmation.PlayerId &&
                         ec.ExpiresAt > DateTimeOffset.UtcNow &&
                         !ec.Used &&
                         !ec.Invalidated)
            .Set(ec => ec.Invalidated, true)
            .UpdateAsync();

        emailConfirmation.Id = await db.InsertWithInt64IdentityAsync(emailConfirmation);

        await db.CommitTransactionAsync();
        await db.DisposeAsync();

        await apiServer.EmailChangeRequested(emailConfirmation.PlayerId,
            oldEmail ?? "",
            newEmail,
            oldEmail ?? newEmail,
            emailConfirmation.ConfirmationUrl);
    }
}
