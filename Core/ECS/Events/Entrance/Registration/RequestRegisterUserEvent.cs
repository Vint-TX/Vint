using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Registration;

[ProtocolId(1438590245672)]
public class RequestRegisterUserEvent : IServerEvent {
    const int MaxRegistrationsFromOneComputer = 5;

    [ProtocolName("Uid")] public string Username { get; private set; } = null!;
    public string EncryptedPasswordDigest { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;
    public bool Subscribed { get; private set; }
    public bool Steam { get; private set; }
    public bool QuickRegistration { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!RegexUtils.IsLoginValid(Username) || !RegexUtils.IsEmailValid(Email)) {
            await connection.Send(new RegistrationFailedEvent());
            return;
        }

        await using (DbConnection db = new()) {
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
                } else
                    banned = true;
            }

            if (banned) {
                await connection.Send(new RegistrationFailedEvent());
                return;
            }

            if (await db.Players.AnyAsync(player => player.Username == Username) ||
                await db.Players.CountAsync(player => player.HardwareFingerprint == HardwareFingerprint) >= MaxRegistrationsFromOneComputer) {
                await connection.Send(new RegistrationFailedEvent());
                return;
            }
        }

        await connection.Register(Username, EncryptedPasswordDigest, Email, HardwareFingerprint, Subscribed, Steam, QuickRegistration);
    }
}
