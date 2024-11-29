using EmbedIO;
using EmbedIO.WebApi;
using Vint.Core.Battles;
using Vint.Core.Config;
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

    [Get("/globalEntities")]
    public IEnumerable<string> GetEntitiesTypeNames() =>
        ConfigManager.GlobalEntitiesTypeNames;

    [Get("/globalEntities/{typeName}")]
    public async Task<string> GetEntities(string typeName) =>
        await ConfigManager.GetGlobalEntitiesJson(typeName) ??
        throw HttpException.NotFound($"No entities found for '{typeName}'");
}
