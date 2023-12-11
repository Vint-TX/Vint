using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Events.Entrance.Registration;

[ProtocolId(1438590245672)]
public class RequestRegisterUserEvent : IServerEvent {
    [ProtocolName("uid")] public string Username { get; private set; } = null!;
    public string EncryptedPasswordDigest { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;
    public bool Subscribed { get; private set; }
    public bool Steam { get; private set; }
    public bool QuickRegistration { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        //todo

        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("Registering player '{Username}'", Username);

        if (Username == "fail") {
            connection.Send(new RegistrationFailedEvent());
            return;
        }

        Encryption encryption = new();

        byte[] passwordHash = encryption.RsaDecrypt(Convert.FromBase64String(EncryptedPasswordDigest));
        byte[] autoLoginToken = new byte[32];
        new Random().NextBytes(autoLoginToken);

        connection.Player = new Player(logger, Username, Email) {
            PasswordHash = passwordHash,
            AutoLoginToken = autoLoginToken,
            HardwareFingerprint = HardwareFingerprint,
            Subscribed = Subscribed
        };

        byte[] encryptedAutoLoginToken = encryption.EncryptAutoLoginToken(autoLoginToken, passwordHash);

        connection.Send(new SaveAutoLoginTokenEvent(Username, encryptedAutoLoginToken));

        connection.Register(
            Username, EncryptedPasswordDigest, Email, HardwareFingerprint, Subscribed, Steam, QuickRegistration);
    }
}
