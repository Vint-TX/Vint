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

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.IsOnline) return;

        ILogger logger = connection.Logger.ForType(GetType());
        logger.Warning("Autologin '{Username}'", Username);

        using DbConnection db = new();
        Player? player = db.Players.SingleOrDefault(player => player.Username == Username);

        Punishment? ban = player?.GetBanInfo();

        List<IPlayerConnection> connections = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline && conn.Player.Username == Username)
            .ToList();

        if (player is not { RememberMe: true } ||
            ban is { Active: true } ||
            connections.Count != 0 ||
            player.HardwareFingerprint != HardwareFingerprint ||
            !player.AutoLoginToken.SequenceEqual(new Encryption().RsaDecrypt(EncryptedToken))) {
            connection.Player = null!;
            connection.Send(new AutoLoginFailedEvent());
            return;
        }

        connection.Player = player;
        connection.Login(false, true, HardwareFingerprint);
    }
}