using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;

namespace Vint.Core.Battles.ArcadeMode;

public abstract class ArcadeModeHandler(
    Battle battle
) {
    public Battle Battle { get; } = battle;
    public ArcadeHandler TypeHandler => (ArcadeHandler)Battle.TypeHandler;

    public abstract Task Setup();

    public virtual Task PlayerEntered(BattlePlayer battlePlayer) => Task.CompletedTask;

    public virtual Task PlayerExited(BattlePlayer battlePlayer) => Task.CompletedTask;
}
