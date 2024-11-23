using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.User;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1437480091995)]
public class LoginByPasswordEvent : IServerEvent {
    public bool RememberMe { get; private set; }
    public string PasswordEncipher { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (connection.IsOnline) return;

        Punishment? ban = await connection.Player.GetBanInfo(HardwareFingerprint, ((SocketPlayerConnection)connection).EndPoint.Address.ToString());

        if (ban is { Active: true }) {
            await connection.Send(new UserBlockedEvent($"You are {ban}"));
            await connection.Send(new LoginFailedEvent());
            return;
        }

        if (!new Encryption()
                .GetLoginPasswordHash(connection.Player.PasswordHash)
                .SequenceEqual(Convert.FromBase64String(PasswordEncipher))) {
            await connection.Send(new InvalidPasswordEvent());
            await connection.Send(new LoginFailedEvent());
            return;
        }

        GameServer server = serviceProvider.GetRequiredService<GameServer>();

        List<IPlayerConnection> connections = server
            .PlayerConnections
            .Values
            .Where(player => player.IsOnline && player.Player.Id == connection.Player.Id)
            .ToList();

        if (connections.Count != 0) {
            await using DbConnection db = new();

            foreach (IPlayerConnection oldConnection in connections) {
                await db.UpdateAsync(oldConnection.Player);
                await oldConnection.Kick("Login from new place");
            }
        }

        await connection.Login(RememberMe, RememberMe, HardwareFingerprint);
    }
}
