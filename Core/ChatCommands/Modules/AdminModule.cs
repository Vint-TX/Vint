using System.Text;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Battle.Bonus;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.ChatCommands.Modules;

[ChatCommandGroup("admin", "Commands for admins", PlayerGroups.Admin)]
public class AdminModule(
    GameServer server
) : ChatCommandModule {
    [ChatCommand("ban", "Ban a player")]
    public async Task Ban(
        ChatCommandContext ctx,
        [Option("username", "Username of player to ban")] string username,
        [Option("duration", "Duration of ban", true)] string? rawDuration = null,
        [WaitingForText, Option("reason", "Reason for ban", true)] string? reason = null) {
        _ = TimeSpanUtils.TryParseDuration(rawDuration, out TimeSpan? duration);

        IPlayerConnection? targetConnection = server
            .PlayerConnections.Values
            .Where(conn => conn.IsLoggedIn)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            if (targetConnection.InLobby) {
                LobbyPlayer lobbyPlayer = targetConnection.LobbyPlayer;

                notifyChat = lobbyPlayer.InRound
                    ? lobbyPlayer.Round.ChatEntity
                    : lobbyPlayer.Lobby.ChatEntity;

                notifiedConnections = ChatUtils
                    .GetReceivers(server, targetConnection, notifyChat)
                    .ToList();
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

        if (!ctx.Connection.Player.IsAdmin &&
            targetPlayer.IsModerator) {
            await ctx.SendPrivateResponse("Moderator cannot punish other moderator");
            return;
        }

        Punishment punishment = await targetPlayer.Ban((targetConnection as SocketPlayerConnection)?.EndPoint.Address.ToString(), reason, duration);
        string punishMessage = $"{username} was {punishment}";
        targetConnection?.Kick(reason);

        await ctx.SendPrivateResponse($"Punishment Id: {punishment.Id}");

        if (notifyChat == null ||
            notifiedConnections == null)
            await ctx.SendPublicResponse(punishMessage);
        else {
            await ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                await ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("unban", "Remove ban from player")]
    public async Task UnBan(ChatCommandContext ctx, [Option("username", "Username of player to unban")] string username) {
        IPlayerConnection? targetConnection = server
            .PlayerConnections
            .Values
            .Where(conn => conn.IsLoggedIn)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection != null) {
            if (targetConnection.InLobby) {
                LobbyPlayer lobbyPlayer = targetConnection.LobbyPlayer;

                notifyChat = lobbyPlayer.InRound
                    ? lobbyPlayer.Round.ChatEntity
                    : lobbyPlayer.Lobby.ChatEntity;

                notifiedConnections = ChatUtils
                    .GetReceivers(server, targetConnection, notifyChat)
                    .ToList();
            }
        } else {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == username);
        }

        if (targetPlayer == null) {
            await ctx.SendPrivateResponse("Player not found");
            return;
        }

        bool successful = await targetPlayer.UnBan();

        if (!successful) {
            await ctx.SendPrivateResponse($"'{username}' is not banned");
            return;
        }

        string punishMessage = $"{username} was unbanned";

        if (notifyChat == null ||
            notifiedConnections == null)
            await ctx.SendPublicResponse(punishMessage);
        else {
            await ctx.SendResponse(punishMessage, notifyChat, notifiedConnections);

            if (ctx.Chat != notifyChat)
                await ctx.SendPrivateResponse(punishMessage);
        }
    }

    [ChatCommand("createInvite", "Create new invite")]
    public async Task CreateInvite(ChatCommandContext ctx, [Option("code", "Code")] string code, [Option("uses", "Maximum uses")] ushort uses) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Code == code);

        if (invite != null) {
            await ctx.SendPrivateResponse($"Already exists: {invite}");
            return;
        }

        invite = new Invite {
            Code = code,
            RemainingUses = uses
        };

        invite.Id = await db.InsertWithInt64IdentityAsync(invite);
        await ctx.SendPrivateResponse($"{invite}");
    }

    [ChatCommand("kickAllFromRound", "Kicks all players in round to lobby"), RequireConditions(ChatCommandConditions.InLobby)]
    public async Task KickAllFromRound(ChatCommandContext ctx) {
        Round? round = ctx.Connection.LobbyPlayer!.Round;

        if (round == null) {
            await ctx.SendPrivateResponse("Round is not started");
            return;
        }

        foreach (Tanker tanker in round.Tankers) {
            await tanker.Send(new KickFromBattleEvent(), tanker.BattleUser);
            await round.RemoveTanker(tanker);
        }
    }

    [ChatCommand("usernames", "Online player usernames")]
    public async Task Usernames(ChatCommandContext ctx) {
        StringBuilder builder = new();
        List<IPlayerConnection> connections = server.PlayerConnections.Values.ToList();

        List<string> onlineUsernames = connections
            .Where(connection => connection.IsLoggedIn)
            .Select(connection => connection.Player.Username)
            .ToList();

        builder.AppendLine($"{connections.Count} players connected, {onlineUsernames.Count} players online:");
        builder.AppendJoin(Environment.NewLine, onlineUsernames);
        await ctx.SendPrivateResponse(builder.ToString());
    }

    [ChatCommand("dropBonus", "Drop bonus"), RequireConditions(ChatCommandConditions.InRound)]
    public async Task DropBonus(
        ChatCommandContext ctx,
        [Option("type", "Type of the bonus")] BonusType bonusType,
        [Option("anonymous", "Drop the gold anonymously", true)] bool anon = true) {
        LobbyPlayer lobbyPlayer = ctx.Connection.LobbyPlayer!;
        Round round = lobbyPlayer.Round!; // todo
        IBonusProcessor? bonusProcessor = round.BonusProcessor;

        if (bonusProcessor == null) {
            await ctx.SendPrivateResponse("Bonuses are disabled in this battle");
            return;
        }

        Tanker? tanker = anon ? null : lobbyPlayer.Tanker;
        bool dropped = await bonusProcessor.ForceDropBonus(bonusType, tanker);

        if (!dropped) {
            await ctx.SendPrivateResponse($"{bonusType} is not dropped");
            return;
        }

        await ctx.SendPrivateResponse($"{bonusType} dropped");
    }

    [ChatCommand("tps", "Show TPS")]
    public async Task TPS(ChatCommandContext ctx) {
        TimeSpan deltaTime = ctx.ServiceProvider.GetRequiredService<GameServer>()
            .DeltaTime;

        await ctx.SendPrivateResponse($"{1 / deltaTime.TotalSeconds} TPS");
    }
}
