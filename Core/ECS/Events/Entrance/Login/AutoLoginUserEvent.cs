using LinqToDB;
using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1438075609642)]
public class AutoLoginUserEvent : IServerEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;
    public byte[] EncryptedToken { get; private set; } = null!;
    public string HardwareFingerprint { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.IsOnline) return;

        ILogger logger = connection.Logger.ForType(GetType());
        logger.Warning("Autologin '{Username}'", Username);

        await using DbConnection db = new();
        Player? player = await db.Players
            .LoadWith(player => player.Modules)
            .LoadWith(player => player.DiscordLink)
            .SingleOrDefaultAsync(player => player.Username == Username);

        if (player == null) {
            await Fail(connection);
            return;
        }

        Punishment? ban = await player.GetBanInfo(HardwareFingerprint, ((SocketPlayerConnection)connection).EndPoint.Address.ToString());
        int connections = connection.Server.PlayerConnections.Values
            .Count(conn => conn.IsOnline && conn.Player.Username == Username);

        if (!player.RememberMe ||
            connections != 0 ||
            ban is { Active: true } ||
            player.HardwareFingerprint != HardwareFingerprint ||
            !player.AutoLoginToken.SequenceEqual(new Encryption().RsaDecrypt(EncryptedToken))) {
            await Fail(connection);
            return;
        }

        connection.Player = player;
        await connection.Login(false, true, HardwareFingerprint);
    }

    static async Task Fail(IPlayerConnection connection) {
        connection.Player = null!;
        await connection.Send(new AutoLoginFailedEvent());
    }
}
