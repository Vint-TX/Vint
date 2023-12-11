using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1437480091995)]
public class LoginByPasswordEvent : IServerEvent {
    public bool RememberMe { get; private set; }
    public string PasswordEncipher { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        //todo

        if (RememberMe) {
            Encryption encryption = new();

            byte[] autoLoginToken = new byte[32];
            new Random().NextBytes(autoLoginToken);

            connection.Player.RememberMe = true;
            connection.Player.HardwareFingerprint = HardwareFingerprint;
            connection.Player.AutoLoginToken = autoLoginToken;

            byte[] encryptedAutoLoginToken = encryption.EncryptAutoLoginToken(autoLoginToken, connection.Player.PasswordHash);

            connection.Send(new SaveAutoLoginTokenEvent(connection.Player.Username, encryptedAutoLoginToken));
        }

        if (connection.Player.IsBanned)
            connection.Send(new LoginFailedEvent());
        else
            connection.Login(RememberMe, PasswordEncipher, HardwareFingerprint);
    }
}
