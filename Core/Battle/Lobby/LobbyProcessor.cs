using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Battle.Autopilot;
using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Battle.Lobby.Impl.Arcade;
using Vint.Core.Battle.Lobby.State;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Properties;
using Vint.Core.Config.MapInformation;
using Vint.Core.Logging;
using Vint.Core.Matchmaking;
using Vint.Core.Quests;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Battle.Lobby;

public class LobbyProcessor(
    IServiceProvider serviceProvider,
    QuestManager questManager
) {
    ILogger Logger { get; } = Log.Logger.ForType<LobbyProcessor>();
    ConcurrentDictionary<long, LobbyBase> LobbiesDict { get; } = [];

    public int Count => LobbiesDict.Count;

    public ICollection<LobbyBase> Lobbies => LobbiesDict.Values;

    public async Task Tick(TimeSpan deltaTime, CancellationToken cancellationToken) {
        foreach (LobbyBase lobby in Lobbies) {
            try {
                await lobby.Tick(deltaTime, cancellationToken);
            } catch (Exception e) {
                Logger.Error(e, "Caught exception while ticking lobby {Id}", lobby.Entity.Id);
            }
        }
    }

    public async Task<CustomLobby> CreateCustom(ClientBattleParams clientParams, IPlayerConnection owner) {
        BattleProperties properties = new(BattleType.Custom, clientParams);
        CustomLobby lobby = new(properties, owner, questManager, CreateBotBuilder()) { PlayerRemoved = PlayerRemoved };
        await lobby.Init();

        LobbiesDict[lobby.Entity.Id] = lobby;
        return lobby;
    }

    public async Task<RatingLobby> CreateRating(BattleProperties properties) {
        RatingLobby lobby = new(properties, questManager, CreateBotBuilder()) { PlayerRemoved = PlayerRemoved };
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
        .FirstOrDefault(state => state.Round.Entity.Id == id)?
        .StateManager.Lobby;

    public LobbyBase? FindByLobbyId(long id) => Lobbies.FirstOrDefault(lobby => lobby.Entity.Id == id);

    ArcadeLobby CreateArcadeByModeType(MapInfo mapInfo, BattleMode battleMode, ArcadeModeType modeType) => modeType switch {
        ArcadeModeType.CosmicBattle => new CosmicLobby(mapInfo, battleMode, questManager, CreateBotBuilder()) { PlayerRemoved = PlayerRemoved },
        ArcadeModeType.QuickPlay => new QuickPlayLobby(mapInfo, battleMode, questManager, CreateBotBuilder()) { PlayerRemoved = PlayerRemoved },
        ArcadeModeType.WithoutDamage => new WithoutDamageLobby(mapInfo, battleMode, questManager, CreateBotBuilder()) { PlayerRemoved = PlayerRemoved },
        ArcadeModeType.FullRandom => new FullRandomLobby(mapInfo, battleMode, questManager, CreateBotBuilder()) { PlayerRemoved = PlayerRemoved },
        _ => throw new ArgumentOutOfRangeException(nameof(modeType), modeType, null)
    };

    void RemoveLobby(long id) => LobbiesDict.TryRemove(id, out _);

    void PlayerRemoved(LobbyBase lobby) {
        if (lobby.Players.Count != 0) return;

        RemoveLobby(lobby.Entity.Id);
        lobby.Dispose();
    }

    BotBuilder CreateBotBuilder() => serviceProvider.GetRequiredService<BotBuilder>();
}
