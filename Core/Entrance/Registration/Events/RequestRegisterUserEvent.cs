using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Email;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Entrance.Registration.Events;

[ProtocolId(1438590245672)]
public class RequestRegisterUserEvent(
    ApiServer apiServer
) : IServerEvent {
    const int MaxRegistrationsPerComputer =
#if DEBUG
        100;
#else
        5;
#endif

    [ProtocolName("Uid")] public string Username { get; private set; } = null!;
    public string EncryptedPasswordDigest { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;
    public bool Subscribed { get; private set; }
    public bool Steam { get; private set; }
    public bool QuickRegistration { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.IsLoggedIn) return;

        Email = Email.Trim();
        EmailValidationResult emailValidationResult = await EmailUtils.Validate(Email, false);

        if (!RegexUtils.IsLoginValid(Username) || emailValidationResult != EmailValidationResult.Vacant) {
            await connection.Send<RegistrationFailedEvent>();
            return;
        }

        DbConnection db = new();

        List<Punishment> punishments = await db.Punishments
            .Where(punishment => punishment.Active &&
                                 punishment.Type == PunishmentType.Ban &&
                                 punishment.HardwareFingerprint == HardwareFingerprint)
            .ToListAsync();

        bool banned = false;

        foreach (Punishment punishment in punishments) {
            if (punishment.EndTime <= DateTimeOffset.UtcNow) {
                punishment.Active = false;
                await db.UpdateAsync(punishment);
            } else banned = true;
        }

        if (banned ||
            await db.IsUsernameTaken(Username) ||
            await db.Players.CountAsync(player => player.HardwareFingerprint == HardwareFingerprint) >= MaxRegistrationsPerComputer) {
            await connection.Send<RegistrationFailedEvent>();
            return;
        }

        await db.DisposeAsync();
        await connection.Register(Username, EncryptedPasswordDigest, Email, HardwareFingerprint, Subscribed, Steam, QuickRegistration);

        EmailConfirmation emailConfirmation = new() {
            PlayerId = connection.Player.Id,
            OldEmail = null,
            NewEmail = Email,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(15),
        };

        db = new DbConnection();
        emailConfirmation.Id = await db.InsertWithInt64IdentityAsync(emailConfirmation);
        await db.DisposeAsync();

        await apiServer.NewPlayerRegistered(connection.Player.Id, emailConfirmation.ConfirmationUrl);
    }
}
