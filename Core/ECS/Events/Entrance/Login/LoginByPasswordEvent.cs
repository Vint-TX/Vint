using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.User;
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

        Punishment? ban = connection.Player.GetBanInfo();

        if (ban is { Active: true }) {
            connection.Send(new UserBlockedEvent($"You are {ban}"));
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

        List<IPlayerConnection> connections = connection.Server.PlayerConnections.Values
            .Where(player => player.IsOnline && player.Player.Id == connection.Player.Id)
            .ToList();

        if (connections.Count != 0) {
            using DbConnection db = new();

            foreach (IPlayerConnection oldConnection in connections) {
                db.Update(oldConnection.Player);
                oldConnection.Kick("Login from new place");
            }
        }

        connection.Login(RememberMe, HardwareFingerprint);
    }
}