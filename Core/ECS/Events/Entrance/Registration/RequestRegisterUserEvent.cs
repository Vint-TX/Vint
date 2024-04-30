using DSharpPlus.Entities;
using Vint.Core.Database;
using Vint.Core.Discord;
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
    [ProtocolName("Email")] public string DiscordUsername { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;
    public bool Subscribed { get; private set; }
    public bool Steam { get; private set; }
    public bool QuickRegistration { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        DiscordBot? discord = connection.Server.DiscordBot;
        DiscordMember? member = discord?.GetMember(DiscordUsername);

        if (!RegexUtils.IsLoginValid(Username) ||
            !RegexUtils.IsDiscordUsernameValid(DiscordUsername) ||
            discord != null && member! == null!) {
            connection.Send(new RegistrationFailedEvent());
            return;
        }

        using (DbConnection db = new()) {
            if (db.Players.Any(player => player.Username == Username) ||
                member! != null! && db.Players.Any(player => player.DiscordId == member.Id) ||
                db.Players.Count(player => player.HardwareFingerprint == HardwareFingerprint) >= MaxRegistrationsFromOneComputer) {
                connection.Send(new RegistrationFailedEvent());
                return;
            }
        }

        connection.Register(
            Username,
            EncryptedPasswordDigest,
            HardwareFingerprint,
            member?.Id ?? 0,
            Subscribed,
            Steam,
            QuickRegistration);
    }
}
