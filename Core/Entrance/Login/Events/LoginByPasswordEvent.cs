using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.ClientSession;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Login.Events;

[ProtocolId(1437480091995)]
public class LoginByPasswordEvent(
    GameServer server
) : IServerEvent {
    public bool RememberMe { get; private set; }
    public string PasswordEncipher { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.IsLoggedIn) return;

        Punishment? ban = await connection.Player.GetBanInfo(HardwareFingerprint, ((SocketPlayerConnection)connection).EndPoint.Address.ToString());

        if (ban is { Active: true }) {
            await connection.Send(new UserBlockedEvent($"You are {ban}"));
            await connection.Send<LoginFailedEvent>();
            return;
        }

        if (!new Encryption()
                .GetLoginPasswordHash(connection.Player.PasswordHash)
                .SequenceEqual(Convert.FromBase64String(PasswordEncipher))) {
            await connection.Send<InvalidPasswordEvent>();
            await connection.Send<LoginFailedEvent>();
            return;
        }

        foreach (IPlayerConnection oldConnection in server.FindConnections(connection.Player.Id))
            await oldConnection.Kick("Login from new place");

        await connection.Login(RememberMe, RememberMe, HardwareFingerprint);
    }
}
