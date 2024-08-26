using System.Collections.Concurrent;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Mines;

public class MineProcessor {
    int _lastIndex;

    ConcurrentDictionary<int, Mine> Mines { get; } = [];

    public int GenerateIndex() => Interlocked.Increment(ref _lastIndex);

    public bool AddMine(IMineEffect effect) => Mines.TryAdd(effect.Index, new Mine(effect));

    public bool RemoveMine(int index) => Mines.Remove(index, out _);

    public void TryTriggerSingle(BattleTank tank) {
        foreach (Mine mine in Mines.Values) {
            if (mine.TryTrigger(tank))
                break;
        }
    }
}
