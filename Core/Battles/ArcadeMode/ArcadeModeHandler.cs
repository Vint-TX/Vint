using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;

namespace Vint.Core.Battles.ArcadeMode;

public abstract class ArcadeModeHandler(
    Battle battle
) {
    public Battle Battle { get; } = battle;
    public ArcadeHandler TypeHandler => (ArcadeHandler)Battle.TypeHandler;

    public abstract void Setup();

    public virtual void PlayerEntered(BattlePlayer battlePlayer) { }

    public virtual void PlayerExited(BattlePlayer battlePlayer) { }
}