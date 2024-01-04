using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Type;

public abstract class TypeHandler(
    Battle battle
) {
    public Battle Battle { get; } = battle;

    public abstract void Setup();

    public abstract void Tick();

    public abstract void PlayerEntered(BattlePlayer player);

    public abstract void PlayerExited(BattlePlayer player);
}