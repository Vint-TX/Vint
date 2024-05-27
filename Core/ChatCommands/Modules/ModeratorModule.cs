using LinqToDB;
using Vint.Core.Battles;
using Vint.Core.Battles.Player;
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
    public async Task Warn(
        ChatCommandContext ctx,
        [Option("username", "Username of player to warn")]
        string username,
        [Option("duration", "Duration of warn", true)]
        string? rawDuration = null,
        [WaitingForText, Option("reason", "Reason for warn", true)]
        string? reason = null) {
        _ = TimeSpanUtils.TryParseDuration(rawDuration, out TimeSpan? duration);

        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections.Values
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
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == username);
        }

        if (targetPlayer == null) {
            await ctx.SendPrivateResponse("Player not found");
            return;
        }

        if (targetPlayer.IsAdmin) {
            await ctx.SendPrivateResponse($"Player '{username}' is admin");
            return;
        }

        if (!ctx.Connection.Player.IsAdmin && targetPlayer.IsModerator) {
            await ctx.SendPrivateResponse("Moderator cannot punish other moderator");
            return;
        }

        Punishment punishment = await targetPlayer.Warn((targetConnection as SocketPlayerConnection)?.EndPoint.Address.ToString(), reason, duration);
        string punishMessage = $"{username} was {punishment}";

        await ctx.SendPrivateResponse($"Punishment Id: {punishment.Id}");

        if (notifyChat == null || notifiedConnections == null)
            await ctx.SendPublicResponse(punishMessage);
        else {
            await ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                await ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("mute", "Mute a player")]
    public async Task Mute(
        ChatCommandContext ctx,
        [Option("username", "Username of player to mute")]
        string username,
        [Option("duration", "Duration of mute", true)]
        string? rawDuration = null,
        [WaitingForText, Option("reason", "Reason for mute", true)]
        string? reason = null) {
        _ = TimeSpanUtils.TryParseDuration(rawDuration, out TimeSpan? duration);

        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections.Values
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
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == username);
        }

        if (targetPlayer == null) {
            await ctx.SendPrivateResponse("Player not found");
            return;
        }

        if (targetPlayer.IsAdmin) {
            await ctx.SendPrivateResponse($"Player '{username}' is admin");
            return;
        }

        if (!ctx.Connection.Player.IsAdmin && targetPlayer.IsModerator) {
            await ctx.SendPrivateResponse("Moderator cannot punish other moderator");
            return;
        }

        Punishment punishment = await targetPlayer.Mute((targetConnection as SocketPlayerConnection)?.EndPoint.Address.ToString(), reason, duration);
        string punishMessage = $"{username} was {punishment}";

        await ctx.SendPrivateResponse($"Punishment Id: {punishment.Id}");

        if (notifyChat == null || notifiedConnections == null)
            await ctx.SendPublicResponse(punishMessage);
        else {
            await ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                await ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("unwarn", "Remove warn from player")]
    public async Task UnWarn(
        ChatCommandContext ctx,
        [Option("username", "Username of player to unwarn")]
        string username,
        [Option("id", "Id of warn")] long id) {
        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections.Values
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
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == username);
        }

        if (targetPlayer == null) {
            await ctx.SendPrivateResponse("Player not found");
            return;
        }

        bool successful = await targetPlayer.UnWarn(id);

        if (!successful) {
            await ctx.SendPrivateResponse($"Cannot find warn with id {id} from '{username}'");
            return;
        }

        string punishMessage = $"{username} was unwarned";

        if (notifyChat == null || notifiedConnections == null)
            await ctx.SendPublicResponse(punishMessage);
        else {
            await ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                await ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("unmute", "Remove mute from player")]
    public async Task UnMute(
        ChatCommandContext ctx,
        [Option("username", "Username of player to unmute")]
        string username) {
        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections.Values
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
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == username);
        }

        if (targetPlayer == null) {
            await ctx.SendPrivateResponse("Player not found");
            return;
        }

        bool successful = await targetPlayer.UnMute();

        if (!successful) {
            await ctx.SendPrivateResponse($"'{username}' is not muted");
            return;
        }

        string punishMessage = $"{username} was unmuted";

        if (notifyChat == null || notifiedConnections == null)
            await ctx.SendPublicResponse(punishMessage);
        else {
            await ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                await ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("kick", "Kick player from server")]
    public async Task Kick(
        ChatCommandContext ctx,
        [Option("username", "Username of player to kick")]
        string username,
        [WaitingForText, Option("reason", "Reason for kick", true)]
        string? reason = null) {
        IPlayerConnection? targetConnection = ctx.Connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection == null) {
            await ctx.SendPrivateResponse("Player is not on the server");
            return;
        }

        if (targetConnection.Player.IsAdmin) {
            await ctx.SendPrivateResponse($"Player '{username}' is admin");
            return;
        }

        if (!ctx.Connection.Player.IsAdmin && targetConnection.Player.IsModerator) {
            await ctx.SendPrivateResponse("Moderator cannot punish other moderator");
            return;
        }

        if (targetConnection.InLobby) {
            Battle battle = targetConnection.BattlePlayer!.Battle;

            notifyChat = targetConnection.BattlePlayer.InBattleAsTank ? battle.BattleChatEntity : battle.LobbyChatEntity;
            notifiedConnections = ChatUtils.GetReceivers(targetConnection, notifyChat).ToList();
        }

        await targetConnection.Kick(reason);
        string punishMessage = $"{username} was kicked for '{reason}'";

        if (notifyChat == null || notifiedConnections == null)
            await ctx.SendPublicResponse(punishMessage);
        else {
            await ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                await ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("dmsg", "Displays a message on player screen")]
    public async Task DisplayMessage(
        ChatCommandContext ctx,
        [Option("username", "Username of player (@a for broadcast, @b for broadcast in battle)")]
        string username,
        [WaitingForText, Option("message", "Message to display")]
        string message) {
        switch (username) {
            case "@a": {
                foreach (IPlayerConnection connection in ctx.Connection.Server.PlayerConnections.Values)
                    await connection.DisplayMessage(message);
                break;
            }

            case "@b":
                if (!ctx.Connection.InLobby || !ctx.Connection.BattlePlayer!.InBattle) return;

                foreach (BattlePlayer battlePlayer in ctx.Connection.BattlePlayer.Battle.Players.Where(player => player.InBattleAsTank))
                    await battlePlayer.PlayerConnection.DisplayMessage(message);
                break;

            default: {
                IPlayerConnection? target = ctx.Connection.Server.PlayerConnections.Values
                    .Where(conn => conn.IsOnline)
                    .SingleOrDefault(conn => conn.Player.Username == username);

                if (target == null) {
                    await ctx.SendPrivateResponse($"Player '{username}' not found");
                    return;
                }

                await target.DisplayMessage(message);
                break;
            }
        }

        await ctx.SendPrivateResponse("Message displayed");
    }
}
