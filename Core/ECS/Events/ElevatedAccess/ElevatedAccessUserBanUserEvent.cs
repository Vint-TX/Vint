using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.ElevatedAccess;

[ProtocolId(1503470104769)]
public class ElevatedAccessUserBanUserEvent : ElevatedAccessUserBasePunishEvent {
    public string Type { get; private set; } = null!;

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.Player.IsAdmin) return;

        IPlayerConnection? targetConnection = connection.Server.PlayerConnections.Values
            .ToList()
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
            using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == Username);
        }

        if (targetPlayer == null) {
            ChatUtils.SendMessage("Player not found", GlobalChat, [connection], null);
            return;
        }

        if (targetPlayer.IsAdmin) {
            ChatUtils.SendMessage($"Player {Username} is admin", GlobalChat, [connection], null);
            return;
        }

        Punishment? punishment = null;
        string? punishMessage = null;

        switch (Type.ToLower()) {
            case "warn": {
                punishment = targetPlayer.Warn(Reason, null);
                break;
            }

            case "mute": {
                punishment = targetPlayer.Mute(Reason, null);
                break;
            }

            case "kick": {
                if (targetConnection == null)
                    ChatUtils.SendMessage("Player is not on the server", GlobalChat, [connection], null);
                else {
                    targetConnection.Kick(Reason);
                    punishMessage = $"{targetPlayer.Username} was kicked for '{Reason}'";
                }

                break;
            }

            default: {
                ChatUtils.SendMessage($"Unexpected type '{Type}'", GlobalChat, [connection], null);
                break;
            }
        }

        if (punishment != null) {
            ChatUtils.SendMessage($"Punishment Id: {punishment.Id}", GlobalChat, [connection], null);
            punishMessage ??= $"{Username} was {punishment}";
        }

        if (punishMessage == null) return;

        if (notifyChat == null || notifiedConnections == null) {
            notifyChat = GlobalChat;
            notifiedConnections = connection.Server.PlayerConnections.Values.ToList();
        }

        ChatUtils.SendMessage(punishMessage, notifyChat, notifiedConnections, null);
    }
}