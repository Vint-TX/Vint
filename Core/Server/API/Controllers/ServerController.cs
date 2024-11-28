using EmbedIO;
using EmbedIO.WebApi;
using Vint.Core.Battles;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.API.Attributes.Methods;
using Vint.Core.Server.API.DTO.Server;
using Vint.Core.Server.Game;

namespace Vint.Core.Server.API.Controllers;

public class ServerController(
    GameServer server,
    IBattleProcessor battleProcessor
) : WebApiController {
    [Get("/count")]
    public CountDTO GetCount() {
        IPlayerConnection[] connections = server.PlayerConnections.Values.ToArray();
        Dictionary<BattleType, int> battles = battleProcessor.Battles
            .GroupBy(battle => battle.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        int connectionsCount = connections.Length;
        int playersCount = connections.Count(connection => connection.IsOnline);

        int matchmakingCount = battles.GetValueOrDefault(BattleType.MatchMaking, 0);
        int arcadeCount = battles.GetValueOrDefault(BattleType.Arcade, 0);
        int customCount = battles.GetValueOrDefault(BattleType.Custom, 0);

        return new CountDTO(connectionsCount, playersCount, matchmakingCount, arcadeCount, customCount);
    }

    [Post("/dmsg")]
    public async Task DisplayMessage(DMsgRequestDTO request) {
        (long playerId, string? message) = request;

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
}
