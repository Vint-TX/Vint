using EmbedIO;
using EmbedIO.WebApi;
using LinqToDB;
using Vint.Core.Battles;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.API.Attributes.Deserialization;
using Vint.Core.Server.API.Attributes.Methods;
using Vint.Core.Server.API.DTO.Player;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Server.API.Controllers;

public class PlayerController(
    GameServer server
) : WebApiController { // todo registration
    [Get("/")]
    public async Task<IEnumerable<PlayerSummaryDTO>> GetPlayers([FromQuery] int from, [FromQuery(@default: 500)] int count) {
        from = Math.Max(0, from - 1);

        await using DbConnection db = new();
        PlayerSummaryDTO[] players = await db.Players
            .Skip(from)
            .Take(count)
            .Select(player => PlayerSummaryDTO.FromPlayer(player))
            .ToArrayAsync();

        return players;
    }

    [Get("/online")]
    public IEnumerable<PlayerSummaryDTO> GetOnlinePlayers() =>
        server.PlayerConnections.Values
            .Where(connection => connection.IsOnline)
            .Select(connection => PlayerSummaryDTO.FromPlayer(connection.Player));

    [Get("/{id}")]
    public async Task<PlayerDetailDTO> GetPlayer(long id) {
        await using DbConnection db = new();
        Player? player = await db.Players.SingleOrDefaultAsync(player => player.Id == id);

        if (player == null)
            throw HttpException.NotFound($"Player with id {id} does not exists");

        return PlayerDetailDTO.FromPlayer(player);
    }

    [Post("/{id}/dmsg")]
    public async Task DisplayMessage(long playerId, [FromBody] string message) {
        if (playerId == -1) {
            foreach (IPlayerConnection connection in server.PlayerConnections.Values)
                await connection.DisplayMessage(message);

            return;
        }

        IPlayerConnection? target = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == playerId);

        if (target == null)
            throw HttpException.NotFound($"Player '{playerId}' not found");

        await target.DisplayMessage(message);
    }

    [Get("/{id}/restorePasswordCode")]
    public object GetRestorePasswordCode(long id) {
        IPlayerConnection? connection = server.PlayerConnections.Values
            .SingleOrDefault(conn => conn.Player.Id == id);

        if (connection == null)
            throw HttpException.NotFound($"Player with id {id} is offline or does not exists");

        if (connection.RestorePasswordCode == null)
            throw HttpException.BadRequest("Player did not request password recovery");

        return new { Code = connection.RestorePasswordCode };
    }

    [Get("/{id}/statistics")]
    public async Task<StatisticsDTO> GetStatistics(long id) {
        await using DbConnection db = new();
        Statistics? statistics = await db.Statistics.SingleOrDefaultAsync(statistics => statistics.PlayerId == id);

        if (statistics == null)
            throw HttpException.NotFound($"Player with id {id} does not exists");

        return StatisticsDTO.FromStatistics(statistics);
    }

    [Post("/{id}/kick")]
    public async Task KickPlayer(long id, [FromBody] string reason) {
        IPlayerConnection? targetConnection = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == id);

        if (targetConnection == null)
            throw HttpException.NotFound($"Player with id {id} does not exists");

        if (targetConnection.Player.IsAdmin)
            throw HttpException.BadRequest($"Player with id {id} is an admin");

        await targetConnection.Kick(reason);
    }

    [Post("/{id}/warn")]
    public async Task WarnPlayer(long id, [FromBody] PunishDTO punish) {
        IPlayerConnection? targetConnection = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == id);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;
        string? ipAddress = null;

        if (targetConnection != null) {
            ipAddress = ((SocketPlayerConnection)targetConnection).EndPoint.Address.ToString();

            if (targetConnection.InLobby) {
                Battle battle = targetConnection.BattlePlayer.Battle;

                notifyChat = targetConnection.BattlePlayer.InBattleAsTank
                    ? battle.BattleChatEntity
                    : battle.LobbyChatEntity;

                notifiedConnections = ChatUtils
                    .GetReceivers(server, targetConnection, notifyChat)
                    .ToList();
            }
        } else {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == id) ??
                           throw HttpException.NotFound($"Player with id {id} does not exists");
        }

        if (targetPlayer!.IsAdmin)
            throw HttpException.BadRequest($"Player with id {id} is an admin");

        Punishment punishment = await targetPlayer.Warn(ipAddress, punish.Reason, punish.Duration);

        if (notifyChat != null && notifiedConnections != null) {
            string punishMessage = $"{targetPlayer.Username} was {punishment}";
            await ChatUtils.SendMessage(punishMessage, notifyChat, notifiedConnections, null);
        }
    }

    [Post("/{id}/mute")]
    public async Task MutePlayer(long id, [FromBody] PunishDTO punish) {
        IPlayerConnection? targetConnection = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == id);

        Player? targetPlayer = targetConnection?.Player;
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;
        string? ipAddress = null;

        if (targetConnection != null) {
            ipAddress = ((SocketPlayerConnection)targetConnection).EndPoint.Address.ToString();

            if (targetConnection.InLobby) {
                Battle battle = targetConnection.BattlePlayer!.Battle;

                notifyChat = targetConnection.BattlePlayer.InBattleAsTank
                    ? battle.BattleChatEntity
                    : battle.LobbyChatEntity;

                notifiedConnections = ChatUtils
                    .GetReceivers(server, targetConnection, notifyChat)
                    .ToList();
            }
        } else {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == id) ??
                           throw HttpException.NotFound($"Player with id {id} does not exists");
        }

        if (targetPlayer!.IsAdmin)
            throw HttpException.BadRequest($"Player with id {id} is an admin");

        Punishment punishment = await targetPlayer.Mute(ipAddress, punish.Reason, punish.Duration);

        if (notifyChat != null && notifiedConnections != null) {
            string punishMessage = $"{targetPlayer.Username} was {punishment}";
            await ChatUtils.SendMessage(punishMessage, notifyChat, notifiedConnections, null);
        }
    }

    [Post("/{id}/ban")]
    public async Task BanPlayer(long id, [FromBody] PunishDTO punish) {
        IPlayerConnection? targetConnection = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == id);

        Player? targetPlayer = targetConnection?.Player;

        if (targetConnection == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == id) ??
                           throw HttpException.NotFound($"Player with id {id} does not exists");
        }

        if (targetPlayer!.IsAdmin)
            throw HttpException.BadRequest($"Player with id {id} is an admin");

        string? ipAddress = null;

        if (targetConnection != null) {
            ipAddress = ((SocketPlayerConnection)targetConnection).EndPoint.Address.ToString();
            await targetConnection.Kick(punish.Reason);
        }

        await targetPlayer.Ban(ipAddress, punish.Reason, punish.Duration);
    }

    [Post("/{id}/unmute")]
    public async Task UnmutePlayer(long id) {
        Player? targetPlayer = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == id)?.Player;

        if (targetPlayer == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == id) ??
                           throw HttpException.NotFound($"Player with id {id} does not exists");
        }

        bool successful = await targetPlayer.UnMute();

        if (!successful)
            throw HttpException.BadRequest("Player is not muted");
    }

    [Post("/{id}/unban")]
    public async Task UnbanPlayer(long id) {
        Player? targetPlayer = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == id)?.Player;

        if (targetPlayer == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Id == id) ??
                           throw HttpException.NotFound($"Player with id {id} does not exists");
        }

        bool successful = await targetPlayer.UnBan();

        if (!successful)
            throw HttpException.BadRequest("Player is not banned");
    }
}
