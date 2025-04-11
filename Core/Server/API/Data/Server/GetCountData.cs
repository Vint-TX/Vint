using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Properties;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.Server.API.Data.Server;

[MessageId(2)]
public class GetCountData(
    GameServer server,
    LobbyProcessor lobbyProcessor
) : IServerData {
    public Task<IClientData> Process() {
        IPlayerConnection[] connections = server.PlayerConnections.Values.ToArray();
        Dictionary<BattleType, int> battles = lobbyProcessor.Lobbies
            .GroupBy(battle => battle.Properties.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        int connectionsCount = connections.Length;
        int playersCount = connections.Count(connection => connection.IsLoggedIn);

        int matchmakingCount = battles.GetValueOrDefault(BattleType.Rating, 0);
        int arcadeCount = battles.GetValueOrDefault(BattleType.Arcade, 0);
        int customCount = battles.GetValueOrDefault(BattleType.Custom, 0);

        return Task.FromResult<IClientData>(new CountData(connectionsCount, playersCount, matchmakingCount, arcadeCount, customCount));
    }
}

[MessageId(3)]
public record CountData(
    int ConnectionsCount,
    int PlayersCount,
    int MatchmakingCount,
    int ArcadeCount,
    int CustomCount
) : IClientData;
