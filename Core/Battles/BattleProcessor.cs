using System.Diagnostics;
using Serilog;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public interface IBattleProcessor {
    public void StartTicking();

    public void PutPlayer(IPlayerConnection connection);

    public Battle? SingleOrDefault(Func<Battle, bool> predicate);

    public Battle? FirstOrDefault(Func<Battle, bool> predicate);

    public Battle? FindBattle(long id);

    public Battle CreateBattle();
}

public class BattleProcessor : IBattleProcessor {
    Dictionary<long, Battle> Battles { get; } = new();

    ILogger Logger { get; } = Log.Logger.ForType(typeof(BattleProcessor));

    public void StartTicking() {
        const double battleTickDuration = 0.01;

        try {
            Stopwatch stopwatch = new();
            double lastBattleTickDuration = 0;

            while (true) {
                stopwatch.Restart();

                foreach (Battle battle in Battles.Values.ToArray())
                    battle.Tick(lastBattleTickDuration);

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

    public void PutPlayer(IPlayerConnection connection) {
        Battle battle = Battles.Values.FirstOrDefault() ?? CreateBattle();

        battle.AddPlayer(connection);
    }

    public Battle? SingleOrDefault(Func<Battle, bool> predicate) => Battles.Values.SingleOrDefault(predicate);

    public Battle? FirstOrDefault(Func<Battle, bool> predicate) => Battles.Values.FirstOrDefault(predicate);

    public Battle? FindBattle(long id) => Battles.GetValueOrDefault(id);

    public Battle CreateBattle() { // todo
        Battle battle = new();
        Battles[battle.Id] = battle;

        return battle;
    }
}