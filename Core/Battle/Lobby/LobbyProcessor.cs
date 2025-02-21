using System.Collections.Concurrent;
using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Lobby.Impl.Arcade;
using Vint.Core.Battle.Matchmaking;
using Vint.Core.Battle.Properties;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Enums;
using Vint.Core.Quests;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Lobby;

public class LobbyProcessor(
    QuestManager questManager
) {
    ConcurrentDictionary<long, LobbyBase> LobbiesDict { get; } = [];

    public int Count => LobbiesDict.Count;

    public ICollection<LobbyBase> Lobbies => LobbiesDict.Values;

    public async Task Tick(TimeSpan deltaTime) {
        foreach (LobbyBase lobby in Lobbies)
            await lobby.Tick(deltaTime);
    }

    public async Task<CustomLobby> CreateCustom(ClientBattleParams clientParams, IPlayerConnection owner) {
        BattleProperties properties = new(BattleType.Custom, TimeSpan.Zero, true, clientParams);
        CustomLobby lobby = new(properties, owner, questManager) { PlayerRemoved = PlayerRemoved };
        await lobby.Init();

        LobbiesDict[lobby.Entity.Id] = lobby;
        return lobby;
    }

    public async Task<RatingLobby> CreateRating(BattleProperties properties) {
        RatingLobby lobby = new(properties, questManager) { PlayerRemoved = PlayerRemoved };
        await lobby.Init();

        LobbiesDict[lobby.Entity.Id] = lobby;
        return lobby;
    }

    public async Task<ArcadeLobby> CreateArcade(MapInfo mapInfo, BattleMode battleMode, ArcadeModeType modeType) {
        ArcadeLobby lobby = CreateArcadeByModeType(mapInfo, battleMode, modeType);
        await lobby.Init();

        LobbiesDict[lobby.Entity.Id] = lobby;
        return lobby;
    }

    public LobbyBase? FindByBattleId(long id) => Lobbies
        .Select(lobby => lobby.StateManager.CurrentState)
        .OfType<Running>()
        .SingleOrDefault(state => state.Round.Entity.Id == id)?
        .StateManager.Lobby;

    public LobbyBase? FindByLobbyId(long id) => Lobbies.SingleOrDefault(lobby => lobby.Entity.Id == id);

    ArcadeLobby CreateArcadeByModeType(MapInfo mapInfo, BattleMode battleMode, ArcadeModeType modeType) => modeType switch {
        ArcadeModeType.CosmicBattle => new CosmicLobby(mapInfo, battleMode, questManager) { PlayerRemoved = PlayerRemoved },
        ArcadeModeType.QuickPlay => new QuickPlayLobby(mapInfo, battleMode, questManager) { PlayerRemoved = PlayerRemoved },
        ArcadeModeType.WithoutDamage => new WithoutDamageLobby(mapInfo, battleMode, questManager) { PlayerRemoved = PlayerRemoved },
        ArcadeModeType.FullRandom => new FullRandomLobby(mapInfo, battleMode, questManager) { PlayerRemoved = PlayerRemoved },
        _ => throw new ArgumentOutOfRangeException(nameof(modeType), modeType, null)
    };


    void RemoveLobby(long id) => LobbiesDict.TryRemove(id, out _);

    void PlayerRemoved(LobbyBase lobby) {
        if (lobby.Players.Count != 0) return;

        RemoveLobby(lobby.Entity.Id);
        lobby.Dispose();
    }
}
