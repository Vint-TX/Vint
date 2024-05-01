using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1512742576673)]
public class ElevatedAccessUserUnbanUserEvent : IServerEvent {
    [ProtocolName("Uid")] public string Username { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin) return;

        IPlayerConnection? targetConnection = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == Username);

        Player? targetPlayer = targetConnection?.Player;

        if (targetConnection == null) {
            await using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == Username);
        }

        if (targetPlayer == null) {
            await ChatUtils.SendMessage("Player not found", ChatUtils.GetChat(connection), [connection], null);
            return;
        }

        await targetPlayer.UnMute();
        await ChatUtils.SendMessage($"'{Username}' unmuted", ChatUtils.GetChat(connection), [connection], null);
    }
}
