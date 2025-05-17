using LinqToDB;
using Vint.Core.Battle.Player;
using Vint.Core.Chat;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Player;
using Vint.Core.Server.API.Data.Status;
using Vint.Core.Server.Game;

namespace Vint.Core.Server.API.Controllers;

public class PlayerController(
    GameServer server
) : IApiController {
    [MessageId(10)]
    public async Task<IClientDTO> GetPlayers(int from, int count = 500) {
        from = Math.Max(0, from - 1);

        await using DbConnection db = new();
        List<PlayerSummaryData> players = await db.Players
            .Skip(from)
            .Take(count)
            .Select(player => PlayerSummaryData.FromPlayer(player))
            .ToListAsync();

        return SuccessDTO.Ok(players);
    }

    [MessageId(11)]
    public IClientDTO GetOnlinePlayers() =>
        SuccessDTO.Ok(server.PlayerConnections.Values
            .Where(connection => connection.IsLoggedIn)
            .Select(connection => PlayerSummaryData.FromPlayer(connection.Player)));

    [MessageId(12)]
    public async Task<IClientDTO> GetPlayer(long id) {
        await using DbConnection db = new();
        Player? player = await db.Players.FirstOrDefaultAsync(player => player.Id == id);

        if (player == null)
            return ErrorDTO.NotFound($"Player with id {id} does not exists");

        return SuccessDTO.Ok(PlayerDetailData.FromPlayer(player));
    }

    [MessageId(13)]
    public async Task<IClientDTO> GetPlayer(string username) {
        await using DbConnection db = new();
        Player? player = await db.Players.FirstOrDefaultAsync(player => player.Username == username);

        if (player == null)
            return ErrorDTO.NotFound($"Player with username {username} does not exists");

        return SuccessDTO.Ok(PlayerDetailData.FromPlayer(player));
    }

    [MessageId(14)]
    public async Task<IClientDTO> DisplayMessage(long playerId, string message) {
        if (playerId == -1) {
            foreach (IPlayerConnection connection in server.PlayerConnections.Values)
                await connection.DisplayMessage(message);

            return SuccessDTO.NoContent();
        }

        IPlayerConnection? target = server.FindConnection(playerId);

        if (target == null)
            return ErrorDTO.NotFound($"Player {playerId} not found");

        await target.DisplayMessage(message);
        return SuccessDTO.NoContent();
    }

    [MessageId(15)]
    public IClientDTO GetRestorePasswordCode(long id) {
        IPlayerConnection? connection = server.FindConnection(id);

        if (connection == null)
            return ErrorDTO.NotFound($"Player with id {id} is offline or does not exists");

        if (connection.RestorePasswordCode == null)
            return ErrorDTO.BadRequest("Player did not request password recovery");

        return SuccessDTO.Ok(new { Code = connection.RestorePasswordCode });
    }

    [MessageId(16)]
    public async Task<IClientDTO> GetStatistics(long id) {
        await using DbConnection db = new();
        Statistics? statistics = await db.Statistics.FirstOrDefaultAsync(statistics => statistics.PlayerId == id);

        if (statistics == null)
            return ErrorDTO.NotFound($"Player with id {id} does not exists");

        return SuccessDTO.Ok(StatisticsData.FromStatistics(statistics));
    }

    [MessageId(18)]
    public async Task<IClientDTO> KickPlayer(long id, string? reason = null) {
        IPlayerConnection? targetConnection = server.FindConnection(id);

        if (targetConnection == null)
            return ErrorDTO.NotFound($"Player with id {id} is offline or does not exists");

        if (targetConnection.Player.IsAdmin)
            return ErrorDTO.BadRequest($"Player with id {id} is an admin");

        await targetConnection.Kick(reason);
        return SuccessDTO.NoContent();
    }

    [MessageId(19)]
    public async Task<IClientDTO> WarnPlayer(long id, string? reason = null, TimeSpan? duration = null) {
        IPlayerConnection? targetConnection = server.FindConnection(id);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;
        string? ipAddress = null;

        if (targetConnection != null) {
            ipAddress = ((SocketPlayerConnection)targetConnection).EndPoint.Address.ToString();

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
            targetPlayer = await db.Players.FirstOrDefaultAsync(player => player.Id == id);

            if (targetPlayer == null)
                return ErrorDTO.NotFound($"Player with id {id} does not exists");
        }

        if (targetPlayer!.IsAdmin)
            return ErrorDTO.BadRequest($"Player with id {id} is an admin");

        Punishment punishment = await targetPlayer.Warn(ipAddress, reason, duration);

        if (notifyChat != null && notifiedConnections != null) {
            string punishMessage = $"{targetPlayer.Username} was {punishment}";
            await ChatUtils.SendMessage(punishMessage, notifyChat, notifiedConnections, null);
        }

        return SuccessDTO.Created(new { punishment.Id }, $"Player {targetPlayer.Username} warned");
    }

    [MessageId(20)]
    public async Task<IClientDTO> MutePlayer(long id, string? reason = null, TimeSpan? duration = null) {
        IPlayerConnection? targetConnection = server.FindConnection(id);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;
        string? ipAddress = null;

        if (targetConnection != null) {
            ipAddress = ((SocketPlayerConnection)targetConnection).EndPoint.Address.ToString();

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
            targetPlayer = await db.Players.FirstOrDefaultAsync(player => player.Id == id);

            if (targetPlayer == null)
                return ErrorDTO.NotFound($"Player with id {id} does not exists");
        }

        if (targetPlayer!.IsAdmin)
            return ErrorDTO.BadRequest($"Player with id {id} is an admin");

        Punishment punishment = await targetPlayer.Mute(ipAddress, reason, duration);

        if (notifyChat != null && notifiedConnections != null) {
            string punishMessage = $"{targetPlayer.Username} was {punishment}";
            await ChatUtils.SendMessage(punishMessage, notifyChat, notifiedConnections, null);
        }

        return SuccessDTO.Created(new { punishment.Id }, $"Player {targetPlayer.Username} muted");
    }

    [MessageId(21)]
    public async Task<IClientDTO> BanPlayer(long id, string? reason = null, TimeSpan? duration = null) {
        IPlayerConnection? targetConnection = server.FindConnection(id);
        Player? targetPlayer = targetConnection?.Player;

        if (targetConnection == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.FirstOrDefaultAsync(player => player.Id == id);

            if (targetPlayer == null)
                return ErrorDTO.NotFound($"Player with id {id} does not exists");
        }

        if (targetPlayer!.IsAdmin)
            return ErrorDTO.BadRequest($"Player with id {id} is an admin");

        string? ipAddress = null;

        if (targetConnection != null) {
            ipAddress = ((SocketPlayerConnection)targetConnection).EndPoint.Address.ToString();
            await targetConnection.Kick(reason);
        }

        Punishment punishment = await targetPlayer.Ban(ipAddress, reason, duration);
        return SuccessDTO.Created(new { punishment.Id }, $"Player {targetPlayer.Username} banned");
    }

    [MessageId(22)]
    public async Task<IClientDTO> UnmutePlayer(long id) {
        Player? targetPlayer = server.FindConnection(id)?.Player;

        if (targetPlayer == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.FirstOrDefaultAsync(player => player.Id == id);

            if (targetPlayer == null)
                return ErrorDTO.NotFound($"Player with id {id} does not exists");
        }

        bool successful = await targetPlayer.UnMute();

        if (!successful)
            return ErrorDTO.BadRequest("Player is not muted");

        return SuccessDTO.NoContent();
    }

    [MessageId(23)]
    public async Task<IClientDTO> UnbanPlayer(long id) {
        Player? targetPlayer = server.FindConnection(id)?.Player;

        if (targetPlayer == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.FirstOrDefaultAsync(player => player.Id == id);

            if (targetPlayer == null)
                return ErrorDTO.NotFound($"Player with id {id} does not exists");
        }

        bool successful = await targetPlayer.UnBan();

        if (!successful)
            return ErrorDTO.BadRequest("Player is not banned");

        return SuccessDTO.NoContent();
    }

    [MessageId(24)]
    public async Task<IClientDTO> ValidateCredentials(string usernameOrEmail, string passwordHash) {
        await using DbConnection db = new();
        var player = await db.Players
            .Where(player => player.Username == usernameOrEmail || player.Email == usernameOrEmail)
            .Select(player => new {
                player.Id,
                player.PasswordHash
            })
            .FirstOrDefaultAsync();

        if (player == null)
            return ErrorDTO.NotFound($"Player with username or email {usernameOrEmail} does not exists");

        if (!Convert.ToHexString(player.PasswordHash).Equals(passwordHash, StringComparison.OrdinalIgnoreCase))
            return ErrorDTO.BadRequest("Invalid password");

        return SuccessDTO.Ok(new { player.Id });
    }
}
