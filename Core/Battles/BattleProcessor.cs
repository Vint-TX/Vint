using ConcurrentCollections;
using Serilog;
using Vint.Core.Battles.Type;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public interface IBattleProcessor {
    IReadOnlyCollection<Battle> Battles { get; }

    Task Tick(TimeSpan deltaTime);

    Task PutPlayerFromMatchmaking(IPlayerConnection connection);

    Task PutArcadePlayer(IPlayerConnection connection, ArcadeModeType mode);

    Battle? SingleOrDefault(Func<Battle, bool> predicate);

    Battle? FirstOrDefault(Func<Battle, bool> predicate);

    Battle? FindByBattleId(long id);

    Battle? FindByLobbyId(long id);

    Battle? FindByIndex(int index);

    Battle CreateMatchmakingBattle();

    Battle CreateArcadeBattle(ArcadeModeType mode);

    Battle CreateCustomBattle(BattleProperties properties, IPlayerConnection owner);
}

public class BattleProcessor(
    IServiceProvider serviceProvider
) : IBattleProcessor {
    ILogger Logger { get; } = Log.Logger.ForType<BattleProcessor>();
    ConcurrentHashSet<Battle> BattleSet { get; } = [];

    public IReadOnlyCollection<Battle> Battles => BattleSet;

    public async Task Tick(TimeSpan deltaTime) {
        foreach (Battle battle in BattleSet) {
            try {
                await battle.Tick(deltaTime);

                if (battle is { WasPlayers: true, Players.Count: 0 }) {
                    Logger.Warning("Removing battle {Id}", battle.LobbyId);
                    BattleSet.TryRemove(battle);
                    battle.Dispose();
                }
            } catch (Exception e) {
                Logger.Error(e, "Caught an exception in battles loop");
            }
        }
    }

    public async Task PutPlayerFromMatchmaking(IPlayerConnection connection) {
        Battle battle = FirstOrDefault(battle => battle is { TypeHandler: MatchmakingHandler, CanAddPlayers: true }) ?? CreateMatchmakingBattle();

        await battle.AddPlayer(connection);
    }

    public async Task PutArcadePlayer(IPlayerConnection connection, ArcadeModeType mode) {
        Battle battle =
            FirstOrDefault(battle => battle is { TypeHandler: ArcadeHandler arcadeHandler, CanAddPlayers: true } &&
                                     arcadeHandler.Mode == mode) ??
            CreateArcadeBattle(mode);

        await battle.AddPlayer(connection);
    }

    public Battle? SingleOrDefault(Func<Battle, bool> predicate) => BattleSet.SingleOrDefault(predicate);

    public Battle? FirstOrDefault(Func<Battle, bool> predicate) => BattleSet.FirstOrDefault(predicate);

    public Battle? FindByBattleId(long id) => SingleOrDefault(battle => battle.Id == id);

    public Battle? FindByLobbyId(long id) => SingleOrDefault(battle => battle.LobbyId == id);

    public Battle? FindByIndex(int index) => BattleSet.ElementAtOrDefault(index);

    public Battle CreateMatchmakingBattle() {
        Battle battle = new(serviceProvider);
        BattleSet.Add(battle);

        return battle;
    }

    public Battle CreateArcadeBattle(ArcadeModeType mode) {
        Battle battle = new(serviceProvider, mode);
        BattleSet.Add(battle);

        return battle;
    }

    public Battle CreateCustomBattle(BattleProperties properties, IPlayerConnection owner) {
        Battle battle = new(serviceProvider, properties, owner);
        BattleSet.Add(battle);

        return battle;
    }
}
