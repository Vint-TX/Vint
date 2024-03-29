using System.Text;
using LinqToDB;
using Vint.Core.Battles;
using Vint.Core.Battles.Bonus;
using Vint.Core.Battles.Player;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ChatCommands.Modules;

[ChatCommandGroup("admin", "Commands for admins", PlayerGroups.Admin)]
public class AdminModule : ChatCommandModule {
    [ChatCommand("ban", "Ban a player")]
    public void Ban(
        ChatCommandContext ctx,
        [Option("username", "Username of player to ban")]
        string username,
        [Option("duration", "Duration of ban", true)]
        string? rawDuration = null,
        [WaitingForText, Option("reason", "Reason for ban", true)]
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

        Punishment punishment = targetPlayer.Ban(reason, duration);
        string punishMessage = $"{username} was {punishment}";
        targetConnection?.Kick(reason);

        ctx.SendPrivateResponse($"Punishment Id: {punishment.Id}");

        if (notifyChat == null || notifiedConnections == null)
            ctx.SendPublicResponse(punishMessage);
        else {
            ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("unban", "Remove ban from player")]
    public void UnBan(
        ChatCommandContext ctx,
        [Option("username", "Username of player to unban")]
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
            using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == username);
        }

        if (targetPlayer == null) {
            ctx.SendPrivateResponse("Player not found");
            return;
        }

        bool successful = targetPlayer.UnBan();

        if (!successful) {
            ctx.SendPrivateResponse($"'{username}' is not banned");
            return;
        }

        string punishMessage = $"{username} was unbanned";

        if (notifyChat == null || notifiedConnections == null)
            ctx.SendPublicResponse(punishMessage);
        else {
            ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("createInvite", "Create new invite")]
    public void CreateInvite(
        ChatCommandContext ctx,
        [Option("code", "Code")] string code,
        [Option("uses", "Maximum uses")] ushort uses) {
        using DbConnection db = new();
        Invite? invite = db.Invites.SingleOrDefault(invite => invite.Code == code);

        if (invite != null) {
            ctx.SendPrivateResponse($"Already exists: {invite}");
            return;
        }

        invite = new Invite {
            Code = code,
            RemainingUses = uses
        };

        invite.Id = db.InsertWithInt64Identity(invite);
        ctx.SendPrivateResponse($"{invite}");
    }

    [ChatCommand("kickAllFromBattle", "Kicks all players in battle to lobby"), RequireConditions(ChatCommandConditions.InLobby)]
    public void KickAllFromBattle(ChatCommandContext ctx) {
        Battle battle = ctx.Connection.BattlePlayer!.Battle;

        foreach (BattlePlayer battlePlayer in battle.Players.Where(battlePlayer => battlePlayer.InBattleAsTank)) {
            battlePlayer.PlayerConnection.Send(new KickFromBattleEvent(), battlePlayer.BattleUser);
            battle.RemovePlayer(battlePlayer);
        }
    }

    [ChatCommand("usernames", "Online player usernames")]
    public void Usernames(ChatCommandContext ctx) {
        StringBuilder builder = new();
        List<IPlayerConnection> connections = ctx.Connection.Server.PlayerConnections.Values.ToList();
        List<string> onlineUsernames = connections
            .Where(connection => connection.IsOnline)
            .Select(connection => connection.Player.Username)
            .ToList();

        builder.AppendLine($"{connections.Count} players connected, {onlineUsernames.Count} players online:");
        builder.AppendJoin(Environment.NewLine, onlineUsernames);
        ctx.SendPrivateResponse(builder.ToString());
    }

    [ChatCommand("dropBonus", "Drop bonus"), RequireConditions(ChatCommandConditions.InBattle)]
    public void DropBonus(
        ChatCommandContext ctx, 
        [Option("type", "Type of the bonus")] BonusType bonusType) {
        bool? isSuccessful = ctx.Connection.BattlePlayer?.Battle.BonusProcessor?.DropBonus(bonusType);

        if (isSuccessful != true) {
            ctx.SendPrivateResponse($"{bonusType} is not dropped");
            return;
        }
        
        ctx.SendPrivateResponse($"{bonusType} dropped");
    }
}