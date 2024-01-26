using Vint.Core.Battles;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ChatCommands.Modules;

[ChatCommandGroup("moderator", "Commands for moderators", PlayerGroups.Moderator)]
public class ModeratorModule : ChatCommandModule {
    [ChatCommand("warn", "Warn a player")]
    public void Warn(
        ChatCommandContext ctx,
        [Option("username", "Username of player to warn")]
        string username,
        [Option("duration", "Duration of warn", true)]
        string? rawDuration = null,
        [WaitingForText, Option("reason", "Reason for warn", true)]
        string? reason = null) {
        _ = TimeSpanUtils.TryParseDuration(rawDuration, out TimeSpan? duration);

        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections
            .ToArray()
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            if (targetConnection.InLobby) {
                Battle battle = targetConnection.BattlePlayer!.Battle;

                notifyChat = targetConnection.BattlePlayer.InBattleAsTank ? battle.BattleChatEntity : battle.LobbyChatEntity;
                notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
            }
        } else {
            using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == username);
        }

        if (targetPlayer == null) {
            ctx.SendPrivateResponse("Player not found");
            return;
        }

        if (targetPlayer.IsAdmin) {
            ctx.SendPrivateResponse($"Player '{username}' is admin");
            return;
        }

        if (!ctx.Connection.Player.IsAdmin && targetPlayer.IsModerator) {
            ctx.SendPrivateResponse("Moderator cannot punish other moderator");
            return;
        }

        Punishment punishment = targetPlayer.Warn(reason, duration);
        string punishMessage = $"{username} was {punishment}";

        ctx.SendPrivateResponse($"Punishment Id: {punishment.Id}");

        if (notifyChat == null || notifiedConnections == null)
            ctx.SendPublicResponse(punishMessage);
        else {
            ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("mute", "Mute a player")]
    public void Mute(
        ChatCommandContext ctx,
        [Option("username", "Username of player to mute")]
        string username,
        [Option("duration", "Duration of mute", true)]
        string? rawDuration = null,
        [WaitingForText, Option("reason", "Reason for mute", true)]
        string? reason = null) {
        _ = TimeSpanUtils.TryParseDuration(rawDuration, out TimeSpan? duration);

        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections
            .ToArray()
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            if (targetConnection.InLobby) {
                Battle battle = targetConnection.BattlePlayer!.Battle;

                notifyChat = targetConnection.BattlePlayer.InBattleAsTank ? battle.BattleChatEntity : battle.LobbyChatEntity;
                notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
            }
        } else {
            using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == username);
        }

        if (targetPlayer == null) {
            ctx.SendPrivateResponse("Player not found");
            return;
        }

        if (targetPlayer.IsAdmin) {
            ctx.SendPrivateResponse($"Player '{username}' is admin");
            return;
        }

        if (!ctx.Connection.Player.IsAdmin && targetPlayer.IsModerator) {
            ctx.SendPrivateResponse("Moderator cannot punish other moderator");
            return;
        }

        Punishment punishment = targetPlayer.Mute(reason, duration);
        string punishMessage = $"{username} was {punishment}";

        ctx.SendPrivateResponse($"Punishment Id: {punishment.Id}");

        if (notifyChat == null || notifiedConnections == null)
            ctx.SendPublicResponse(punishMessage);
        else {
            ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("unwarn", "Remove warn from player")]
    public void UnWarn(
        ChatCommandContext ctx,
        [Option("username", "Username of player to unwarn")]
        string username,
        [Option("id", "Id of warn")] long id) {
        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections
            .ToArray()
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            if (targetConnection.InLobby) {
                Battle battle = targetConnection.BattlePlayer!.Battle;

                notifyChat = targetConnection.BattlePlayer.InBattleAsTank ? battle.BattleChatEntity : battle.LobbyChatEntity;
                notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
            }
        } else {
            using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == username);
        }

        if (targetPlayer == null) {
            ctx.SendPrivateResponse("Player not found");
            return;
        }

        bool successful = targetPlayer.UnWarn(id);

        if (!successful) {
            ctx.SendPrivateResponse($"Cannot find warn with id {id} from '{username}'");
            return;
        }

        string punishMessage = $"{username} was unwarned";

        if (notifyChat == null || notifiedConnections == null)
            ctx.SendPublicResponse(punishMessage);
        else {
            ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("unmute", "Remove mute from player")]
    public void UnMute(
        ChatCommandContext ctx,
        [Option("username", "Username of player to unmute")]
        string username) {
        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections
            .ToArray()
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            if (targetConnection.InLobby) {
                Battle battle = targetConnection.BattlePlayer!.Battle;

                notifyChat = targetConnection.BattlePlayer.InBattleAsTank ? battle.BattleChatEntity : battle.LobbyChatEntity;
                notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
            }
        } else {
            using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == username);
        }

        if (targetPlayer == null) {
            ctx.SendPrivateResponse("Player not found");
            return;
        }

        bool successful = targetPlayer.UnMute();

        if (!successful) {
            ctx.SendPrivateResponse($"'{username}' is not muted");
            return;
        }

        string punishMessage = $"{username} was unmuted";

        if (notifyChat == null || notifiedConnections == null)
            ctx.SendPublicResponse(punishMessage);
        else {
            ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("kick", "Kick player from server")]
    public void Kick(
        ChatCommandContext ctx,
        [Option("username", "Username of player to kick")]
        string username,
        [WaitingForText, Option("reason", "Reason for kick", true)]
        string? reason = null) {
        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections
            .ToArray()
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection == null) {
            ctx.SendPrivateResponse("Player is not on the server");
            return;
        }

        if (targetConnection.Player.IsAdmin) {
            ctx.SendPrivateResponse($"Player '{username}' is admin");
            return;
        }

        if (!ctx.Connection.Player.IsAdmin && targetConnection.Player.IsModerator) {
            ctx.SendPrivateResponse("Moderator cannot punish other moderator");
            return;
        }

        if (targetConnection.InLobby) {
            Battle battle = targetConnection.BattlePlayer!.Battle;

            notifyChat = targetConnection.BattlePlayer.InBattleAsTank ? battle.BattleChatEntity : battle.LobbyChatEntity;
            notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
        }

        targetConnection.Kick(reason);
        string punishMessage = $"{username} was kicked for '{reason}'";

        if (notifyChat == null || notifiedConnections == null)
            ctx.SendPublicResponse(punishMessage);
        else {
            ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                ctx.SendPrivateResponse(punishMessage);
        }
    }
}