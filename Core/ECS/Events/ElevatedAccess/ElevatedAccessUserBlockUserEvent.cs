using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1503470140938), Obsolete]
public class ElevatedAccessUserBlockUserEvent : ElevatedAccessUserBasePunishEvent {
    public override async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin) return;

        IPlayerConnection? targetConnection = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == Username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            notifyChat = ChatUtils.GetChat(targetConnection);
            notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
        } else {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == Username);
        }

        if (targetPlayer == null) {
            await ChatUtils.SendMessage("Player not found", ChatUtils.GetChat(connection), [connection], null);
            return;
        }

        if (targetPlayer.IsAdmin) {
            await ChatUtils.SendMessage($"Player {Username} is admin", ChatUtils.GetChat(connection), [connection], null);
            return;
        }

        Punishment punishment = await targetPlayer.Ban(((SocketPlayerConnection)targetConnection!).EndPoint.Address.ToString(), Reason, null);
        targetConnection?.Kick(Reason);

        if (notifyChat == null || notifiedConnections == null) {
            notifyChat = ChatUtils.GlobalChat;
            notifiedConnections = connection.Server.PlayerConnections.Values.ToList();
        }

        await ChatUtils.SendMessage($"{Username} was {punishment}", notifyChat, notifiedConnections, null);
        await ChatUtils.SendMessage($"Punishment Id: {punishment.Id}", ChatUtils.GetChat(connection), [connection], null);
    }
}
