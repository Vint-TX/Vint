using EmbedIO;
using EmbedIO.WebApi;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Server.API.Attributes.Deserialization;
using Vint.Core.Server.API.Attributes.Methods;

namespace Vint.Core.Server.API.Controllers;

public class PlayerController : WebApiController { // todo registration
    [Get("/")]
    public async Task<IEnumerable<PlayerListDTO>> GetPlayers([FromQuery] int from, [FromQuery(@default: 500)] int count) {
        from = Math.Max(0, from - 1);

        await using DbConnection db = new();
        PlayerListDTO[] players = await db.Players
            .Skip(from)
            .Take(count)
            .Select(player => PlayerListDTO.FromPlayer(player))
            .ToArrayAsync();

        return players;
    }

    [Get("/{id}")]
    public async Task<PlayerDetailDTO> GetPlayer(long id) {
        await using DbConnection db = new();
        Player? player = await db.Players.SingleOrDefaultAsync(player => player.Id == id);

        if (player == null) {
            throw HttpException.NotFound($"Player with id {id} was not found");
        }

        return PlayerDetailDTO.FromPlayer(player);
    }
}

public record PlayerListDTO(
    long Id,
    string Username,
    string Email
) {
    public static PlayerListDTO FromPlayer(Player player) =>
        new(player.Id,
            player.Username,
            player.Email);
}

public record PlayerDetailDTO(
    long Id,
    string Username,
    string Email,
    ulong DiscordId,
    bool Subscribed,
    string CountryCode,
    long AvatarId,
    long Crystals,
    long XCrystals,
    long Experience,
    int Rank,
    uint Reputation,
    string League,
    string Fraction,
    DateTimeOffset RegistrationTime,
    DateTimeOffset LastLoginTime
) {
    public static PlayerDetailDTO FromPlayer(Player player) =>
        new(player.Id,
            player.Username,
            player.Email,
            player.DiscordUserId,
            player.Subscribed,
            player.CountryCode,
            player.CurrentAvatarId,
            player.Crystals,
            player.XCrystals,
            player.Experience,
            player.Rank,
            player.Reputation,
            player.League.ToString(),
            player.FractionName,
            player.RegistrationTime,
            player.LastLoginTime);
}
