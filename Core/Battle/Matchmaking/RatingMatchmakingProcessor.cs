using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Matchmaking;

public class RatingMatchmakingProcessor( // todo matchmaking system
    LobbyProcessor lobbyProcessor
) {
    IEnumerable<RatingLobby> AvailableLobbies => lobbyProcessor.Lobbies.OfType<RatingLobby>().Where(FilterLobbies);

    public async Task EnqueuePlayer(IPlayerConnection connection) {
        RatingLobby? lobby = AvailableLobbies.FirstOrDefault();

        if (lobby == null) {
            BattleProperties properties = GenerateProperties();
            lobby = await lobbyProcessor.CreateRating(properties);
        }

        await lobby.AddPlayer(connection);
    }

    public async Task TryDequeuePlayer(IPlayerConnection connection) { // right now it is just removing player from its lobby
        if (!connection.InLobby)
            return;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;

        if (lobbyPlayer.Lobby is not RatingLobby lobby)
            return;

        if (lobbyPlayer.InRound) // not sure if this is needed but idfc
            await lobbyPlayer.Round.RemoveTanker(lobbyPlayer.Tanker);

        await lobby.RemovePlayer(lobbyPlayer);
    }

    static bool FilterLobbies(RatingLobby lobby) =>
        lobby.Players.Count < lobby.Properties.MaxPlayers &&
        lobby.StateManager.CurrentState is not Ended &&
        lobby.StateManager.CurrentState is not Running { Round.Remaining.TotalMinutes: <= 0 };

    static BattleProperties GenerateProperties() {
        MapInfo mapInfo = GetRandomMapInfo();
        ClientBattleParams battleParams = new(GetRandomMode(), GravityType.Earth, mapInfo, false, true, false, 10);

        return new BattleProperties(BattleType.Rating, TimeSpan.FromMinutes(1), true, battleParams);
    }

    static MapInfo GetRandomMapInfo() => ConfigManager.MapInfos.Where(mapInfo => mapInfo.Matchmaking).ToArray().RandomElement();

    static BattleMode GetRandomMode() {
        Dictionary<BattleMode, double> modesProbability = ConfigManager.CommonMapInfo.ModesProbability;

        double totalProbability = modesProbability.Values.Sum();
        double randomValue = Random.Shared.NextDouble() * totalProbability;

        double cumulative = 0;

        foreach ((BattleMode mode, double probability) in modesProbability) {
            cumulative += probability;

            if (randomValue <= cumulative)
                return mode;
        }

        return modesProbability.Keys.Last();
    }
}
