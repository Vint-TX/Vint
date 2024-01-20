using LinqToDB;
using Vint.Core.Database;
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
        if (connection.IsOnline) return;

        if (connection.Player.IsBanned) {
            connection.Send(new LoginFailedEvent());
            return;
        }

        Encryption encryption = new();

        if (!encryption.GetLoginPasswordHash(connection.Player.PasswordHash)
                .SequenceEqual(Convert.FromBase64String(PasswordEncipher))) {
            connection.Send(new InvalidPasswordEvent());
            connection.Send(new LoginFailedEvent());
            return;
        }

        List<IPlayerConnection> connections = connection.Server.PlayerConnections
            .Where(player => player.IsOnline && player.Player.Id == connection.Player.Id)
            .ToList();

        if (connections.Count > 1) {
            using DbConnection db = new();

            foreach (IPlayerConnection oldConnection in connections) {
                db.Update(oldConnection.Player);
                ((PlayerConnection)oldConnection).Disconnect();
            }
        }

        connection.Login(RememberMe, HardwareFingerprint);
    }
}