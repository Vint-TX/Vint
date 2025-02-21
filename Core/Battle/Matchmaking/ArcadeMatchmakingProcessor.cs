using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Lobby.Impl.Arcade;
using Vint.Core.Battle.Player;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Matchmaking;

public class ArcadeMatchmakingProcessor( // todo matchmaking system
    LobbyProcessor lobbyProcessor
) {
    public async Task EnqueuePlayer(IPlayerConnection connection, ArcadeModeType modeType) {
        ArcadeLobby lobby = GetAvailableLobbies(modeType).FirstOrDefault() ??
                             await lobbyProcessor.CreateArcade(GetRandomMapInfo(), GetRandomMode(), modeType);

        await lobby.AddPlayer(connection);
    }

    public async Task TryDequeuePlayer(IPlayerConnection connection) { // right now it is just removing player from its lobby
        if (!connection.InLobby)
            return;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;

        if (lobbyPlayer.Lobby is not ArcadeLobby lobby)
            return;

        if (lobbyPlayer.InRound) // not sure if this is needed but idfc
            await lobbyPlayer.Round.RemoveTanker(lobbyPlayer.Tanker);

        await lobby.RemovePlayer(lobbyPlayer);
    }

    IEnumerable<ArcadeLobby> GetAvailableLobbies(ArcadeModeType modeType) =>
        lobbyProcessor.Lobbies.OfType<ArcadeLobby>()
            .Where(FilterLobbies)
            .Where(lobby => lobby.ArcadeType == modeType);

    static bool FilterLobbies(ArcadeLobby lobby) =>
        lobby.Players.Count < lobby.Properties.MaxPlayers &&
        lobby.StateManager.CurrentState is not Ended &&
        lobby.StateManager.CurrentState is not Running { Round.Remaining.TotalMinutes: <= 2 };

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

public enum ArcadeModeType {
    CosmicBattle,
    DeathMatch,
    TeamDeathMatch,
    CaptureTheFlag,
    QuickPlay,
    WithoutDamage,
    FullRandom
}
