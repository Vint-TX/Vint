using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1437480091995)]
public class LoginByPasswordEvent : IServerEvent {
    public bool RememberMe { get; private set; }
    public string PasswordEncipher { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        //todo

        Encryption encryption = new();

        if (!encryption.GetLoginPasswordHash(connection.Player.PasswordHash)
                .SequenceEqual(Convert.FromBase64String(PasswordEncipher))) {
            connection.Send(new InvalidPasswordEvent());
            connection.Send(new LoginFailedEvent());

            return;
        }

        if (connection.Player.IsBanned)
            connection.Send(new LoginFailedEvent());
        else
            connection.Login(RememberMe, HardwareFingerprint);
    }
}