using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.Server;

namespace Vint.Core.Battles.Mode;

public abstract class ModeHandler(
    Battle battle
) {
    public Battle Battle { get; } = battle;

    public abstract BattleMode BattleMode { get; }

    public abstract void Tick();

    public abstract SpawnPoint GetRandomSpawnPoint();

    public abstract void OnStarted();

    public abstract void OnFinished();

    public abstract BattlePlayer SetupBattlePlayer(IPlayerConnection player);

    public abstract void RemoveBattlePlayer(BattlePlayer player);

    public abstract void PlayerEntered(BattlePlayer player);

    public abstract void PlayerExited(BattlePlayer player);
}