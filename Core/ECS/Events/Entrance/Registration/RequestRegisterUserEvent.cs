using Vint.Core.Database;
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

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!RegexUtils.IsLoginValid(Username) || !RegexUtils.IsEmailValid(Email)) {
            connection.Send(new RegistrationFailedEvent());
            return;
        }

        using (DbConnection db = new()) {
            if (db.Players.Any(player => player.Username == Username) ||
                db.Players.Count(player => player.HardwareFingerprint == HardwareFingerprint) >= MaxRegistrationsFromOneComputer) {
                connection.Send(new RegistrationFailedEvent());
                return;
            }
        }

        connection.Register(
            Username,
            EncryptedPasswordDigest,
            Email,
            HardwareFingerprint,
            Subscribed,
            Steam,
            QuickRegistration);
    }
}