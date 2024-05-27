using System.Diagnostics;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Type;

public abstract class TypeHandler(
    Battle battle
) {
    public Battle Battle { get; } = battle;

    public abstract Task Setup();

    public abstract Task Tick();

    public abstract Task PlayerEntered(BattlePlayer battlePlayer);

    public abstract Task PlayerExited(BattlePlayer battlePlayer);

    protected static BattleMode GetRandomMode() => Random.Shared.NextDouble() switch {
        < 0.45 => BattleMode.CTF,
        < 0.80 => BattleMode.TDM,
        < 1 => BattleMode.DM,
        _ => throw new UnreachableException()
    };
}
