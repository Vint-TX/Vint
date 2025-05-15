using Newtonsoft.Json.Linq;
using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Properties;
using Vint.Core.Config;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Server;
using Vint.Core.Server.API.Data.Status;
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
            .GroupBy(battle => battle.Properties.GetValue(BattleProperty.Type))
            .ToDictionary(g => g.Key, g => g.Count());

        int connectionsCount = connections.Length;
        int playersCount = connections.Count(connection => connection.IsLoggedIn);

        int matchmakingCount = battles.GetValueOrDefault(BattleType.Rating, 0);
        int arcadeCount = battles.GetValueOrDefault(BattleType.Arcade, 0);
        int customCount = battles.GetValueOrDefault(BattleType.Custom, 0);

        return SuccessDTO.Ok(data: new CountData(connectionsCount, playersCount, matchmakingCount, arcadeCount, customCount));
    }

    [MessageId(5)]
    public IClientDTO GetEntitiesTypeNames() =>
        SuccessDTO.Ok(data: ConfigManager.GlobalEntitiesTypeNames);

    [MessageId(6)]
    public async Task<IClientDTO> GetEntities(string typeName) {
        string? json = await ConfigManager.GetGlobalEntitiesJson(typeName);

        if (json == null)
            return ErrorDTO.NotFound($"No entities found for '{typeName}'");

        return SuccessDTO.Ok(data: JArray.Parse(json));
    }
}
