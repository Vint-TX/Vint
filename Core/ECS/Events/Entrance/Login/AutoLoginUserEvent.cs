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

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.IsOnline) return;

        ILogger logger = connection.Logger.ForType(GetType());
        logger.Warning("Autologin '{Username}'", Username);

        using DbConnection db = new();
        Player? player = db.Players.SingleOrDefault(player => player.Username == Username);

        if (player == null || player.IsBanned || player.HardwareFingerprint != HardwareFingerprint) {
            connection.Send(new AutoLoginFailedEvent());
            return;
        }

        connection.Player = player;

        if (player.AutoLoginToken.SequenceEqual(new Encryption().RsaDecrypt(EncryptedToken))) {
            List<IPlayerConnection> connections = connection.Server.PlayerConnections
                .Where(conn => conn.IsOnline && conn.Player.Id == connection.Player.Id)
                .ToList();

            if (connections.Count > 1) {
                foreach (IPlayerConnection oldConnection in connections) {
                    db.Update(oldConnection.Player);
                    ((PlayerConnection)oldConnection).Disconnect();
                }
            }

            connection.Login(false, HardwareFingerprint);
        } else connection.Send(new AutoLoginFailedEvent());
    }
}