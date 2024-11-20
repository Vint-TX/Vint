using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1503470140938), Obsolete]
public class ElevatedAccessUserBlockUserEvent : ElevatedAccessUserBasePunishEvent {
    public override async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin) return;

        GameServer server = serviceProvider.GetRequiredService<GameServer>();

        IPlayerConnection? targetConnection = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == Username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            notifyChat = ChatUtils.GetChat(targetConnection);
            notifiedConnections = ChatUtils.GetReceivers(server, targetConnection, notifyChat).ToList();
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
            notifiedConnections = server.PlayerConnections.Values.ToList();
        }

        await ChatUtils.SendMessage($"{Username} was {punishment}", notifyChat, notifiedConnections, null);
        await ChatUtils.SendMessage($"Punishment Id: {punishment.Id}", ChatUtils.GetChat(connection), [connection], null);
    }
}
