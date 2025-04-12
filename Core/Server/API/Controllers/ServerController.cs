using Newtonsoft.Json.Linq;
using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Properties;
using Vint.Core.Config;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.DTO.Base;
using Vint.Core.Server.API.DTO.Error;
using Vint.Core.Server.API.DTO.Server;
using Vint.Core.Server.Game;

namespace Vint.Core.Server.API.Controllers;

public class ServerController(
    GameServer server,
    LobbyProcessor lobbyProcessor
) : IApiController {
    [MessageId(4)]
    public IClientDTO GetCount() {
        IPlayerConnection[] connections = server.PlayerConnections.Values.ToArray();
        Dictionary<BattleType, int> battles = lobbyProcessor.Lobbies
            .GroupBy(battle => battle.Properties.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        int connectionsCount = connections.Length;
        int playersCount = connections.Count(connection => connection.IsLoggedIn);

        int matchmakingCount = battles.GetValueOrDefault(BattleType.Rating, 0);
        int arcadeCount = battles.GetValueOrDefault(BattleType.Arcade, 0);
        int customCount = battles.GetValueOrDefault(BattleType.Custom, 0);

        return new CountDTO(connectionsCount, playersCount, matchmakingCount, arcadeCount, customCount);
    }

    [MessageId(5)]
    public EnumerableClientDTO<string> GetEntitiesTypeNames() =>
        new(ConfigManager.GlobalEntitiesTypeNames);

    [MessageId(6)]
    public async Task<EnumerableClientDTO<JToken>> GetEntities(string typeName) {
        string? json = await ConfigManager.GetGlobalEntitiesJson(typeName);

        // if (json == null)
        //     return new ErrorDTO(404, $"No entities found for '{typeName}'", null);

        return new EnumerableClientDTO<JToken>(JArray.Parse(json!));
    }
}
