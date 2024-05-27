using Vint.Core.Battles.Player;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Bonus;

public abstract class BonusState(
    BonusStateManager stateManager
) : State {
    public override BonusStateManager StateManager { get; } = stateManager;
    protected BonusBox Bonus => StateManager.Bonus;
    protected Battle Battle => Bonus.Battle;
}

public class None(
    BonusStateManager stateManager
) : BonusState(stateManager);

public class Cooldown(
    BonusStateManager stateManager,
    TimeSpan? duration = null
) : BonusState(stateManager) {
    TimeSpan Duration { get; } = duration ?? TimeSpan.FromMinutes(2);
    DateTimeOffset TimeToSpawn { get; set; }

    public override async Task Start() {
        await base.Start();

        TimeToSpawn = DateTimeOffset.UtcNow + Duration;
    }

    public override async Task Tick() {
        if (DateTimeOffset.UtcNow >= TimeToSpawn)
            await Bonus.Spawn();

        await base.Tick();
    }
}

public class Spawned(
    BonusStateManager stateManager
) : BonusState(stateManager) {
    public DateTimeOffset SpawnTime { get; private set; }

    public override async Task Start() {
        await base.Start();

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
            await battlePlayer.PlayerConnection.Share(Bonus.Entity!);

        SpawnTime = DateTimeOffset.UtcNow;
    }
}
