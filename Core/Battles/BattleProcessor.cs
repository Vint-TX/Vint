using System.Diagnostics;
using Serilog;
using Vint.Core.Battles.States;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public interface IBattleProcessor {
    public int BattlesCount { get; }

    public void StartTicking();

    public void PutPlayerFromMatchmaking(IPlayerConnection connection);

    public Battle? SingleOrDefault(Func<Battle, bool> predicate);

    public Battle? FirstOrDefault(Func<Battle, bool> predicate);

    public Battle? FindByBattleId(long id);

    public Battle? FindByLobbyId(long id);

    public Battle? FindByIndex(int index);

    public Battle CreateMatchmakingBattle();

    public Battle CreateCustomBattle(BattleProperties properties, IPlayerConnection owner);
}

public class BattleProcessor : IBattleProcessor {
    Dictionary<long, Battle> Battles { get; } = new();

    ILogger Logger { get; } = Log.Logger.ForType(typeof(BattleProcessor));

    public int BattlesCount => Battles.Count;

    public void StartTicking() {
        const double battleTickDuration = 0.01;

        try {
            Stopwatch stopwatch = new();
            double lastBattleTickDuration = 0;

            while (true) {
                stopwatch.Restart();

                foreach (Battle battle in Battles.Values.ToArray()) {
                    battle.Tick(lastBattleTickDuration);

                    if (battle is { WasPlayers: true, Players.Count: 0 } or
                        { IsCustom: false, StateManager.CurrentState: Ended })
                        Battles.Remove(battle.Id);
                }

                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                stopwatch.Start();

                if (elapsed.TotalSeconds < battleTickDuration)
                    Thread.Sleep(TimeSpan.FromSeconds(battleTickDuration) - elapsed);

                stopwatch.Stop();
                lastBattleTickDuration = stopwatch.Elapsed.TotalSeconds;
            }
        } catch (Exception e) {
            Logger.Fatal(e, "Fatal error happened in battles tick loop");
            throw;
        }
    }

    public void PutPlayerFromMatchmaking(IPlayerConnection connection) {
        Battle battle = FirstOrDefault(battle => battle is { IsCustom: false, CanAddPlayers: true }) ?? CreateMatchmakingBattle();

        battle.AddPlayer(connection);
    }

    public Battle? SingleOrDefault(Func<Battle, bool> predicate) => Battles.Values.SingleOrDefault(predicate);

    public Battle? FirstOrDefault(Func<Battle, bool> predicate) => Battles.Values.FirstOrDefault(predicate);

    public Battle? FindByBattleId(long id) => Battles.GetValueOrDefault(id);

    public Battle? FindByLobbyId(long id) => SingleOrDefault(battle => battle.LobbyId == id);

    public Battle? FindByIndex(int index) => Battles.Values.ElementAtOrDefault(index);

    public Battle CreateMatchmakingBattle() {
        Battle battle = new();
        Battles[battle.Id] = battle;

        return battle;
    }

    public Battle CreateCustomBattle(BattleProperties properties, IPlayerConnection owner) {
        Battle battle = new(properties, owner);
        Battles[battle.Id] = battle;

        return battle;
    }
}