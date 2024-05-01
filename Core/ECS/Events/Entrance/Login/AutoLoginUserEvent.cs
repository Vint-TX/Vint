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
            .SingleOrDefaultAsync(player => player.Username == Username);

        if (player == null) {
            Fail(connection);
            return;
        }

        Punishment? ban = await player.GetBanInfo();
        int connections = connection.Server.PlayerConnections.Values
            .Count(conn => conn.IsOnline && conn.Player.Username == Username);

        if (!player.RememberMe ||
            connections != 0 ||
            ban is { Active: true } ||
            player.HardwareFingerprint != HardwareFingerprint ||
            !player.AutoLoginToken.SequenceEqual(new Encryption().RsaDecrypt(EncryptedToken))) {
            Fail(connection);
            return;
        }

        connection.Player = player;
        await connection.Login(false, true, HardwareFingerprint);
    }

    static void Fail(IPlayerConnection connection) {
        connection.Player = null!;
        connection.Send(new AutoLoginFailedEvent());
    }
}
