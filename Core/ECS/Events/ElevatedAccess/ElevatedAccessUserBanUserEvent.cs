using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1503470104769), Obsolete]
public class ElevatedAccessUserBanUserEvent : ElevatedAccessUserBasePunishEvent {
    public string Type { get; private set; } = null!;

    public override async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin) return;

        IPlayerConnection? targetConnection = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == Username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            if (targetConnection.InLobby) {
                Battles.Battle battle = targetConnection.BattlePlayer!.Battle;

                notifyChat = targetConnection.BattlePlayer.InBattleAsTank ? battle.BattleChatEntity : battle.LobbyChatEntity;
                notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
            }
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

        Punishment? punishment = null;
        string? punishMessage = null;

        switch (Type.ToLower()) {
            case "warn": {
                punishment = await targetPlayer.Warn(((SocketPlayerConnection)targetConnection!).EndPoint.Address.ToString(), Reason, null);
                break;
            }

            case "mute": {
                punishment = await targetPlayer.Mute(((SocketPlayerConnection)targetConnection!).EndPoint.Address.ToString(), Reason, null);
                break;
            }

            case "kick": {
                if (targetConnection == null)
                    await ChatUtils.SendMessage("Player is not on the server", ChatUtils.GetChat(connection), [connection], null);
                else {
                    await targetConnection.Kick(Reason);
                    punishMessage = $"{targetPlayer.Username} was kicked for '{Reason}'";
                }

                break;
            }

            default: {
                await ChatUtils.SendMessage($"Unexpected type '{Type}'", ChatUtils.GetChat(connection), [connection], null);
                break;
            }
        }

        if (punishment != null) {
            await ChatUtils.SendMessage($"Punishment Id: {punishment.Id}", ChatUtils.GetChat(connection), [connection], null);
            punishMessage ??= $"{Username} was {punishment}";
        }

        if (punishMessage == null) return;

        if (notifyChat == null || notifiedConnections == null) {
            notifyChat = ChatUtils.GlobalChat;
            notifiedConnections = connection.Server.PlayerConnections.Values.ToList();
        }

        await ChatUtils.SendMessage(punishMessage, notifyChat, notifiedConnections, null);
    }
}
