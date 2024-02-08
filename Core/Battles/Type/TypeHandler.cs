using System.Diagnostics;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Type;

public abstract class TypeHandler(
    Battle battle
) {
    public Battle Battle { get; } = battle;

    public abstract void Setup();

    public abstract void Tick();

    public abstract void PlayerEntered(BattlePlayer battlePlayer);

    public abstract void PlayerExited(BattlePlayer battlePlayer);

    protected static BattleMode GetRandomMode() => Random.Shared.NextDouble() switch {
        < 0.34 => BattleMode.CTF,
        < 0.67 => BattleMode.TDM,
        < 1 => BattleMode.DM,
        _ => throw new UnreachableException()
    };
}