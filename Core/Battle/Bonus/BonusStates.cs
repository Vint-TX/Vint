using Vint.Core.Battle.Rounds;
using Vint.Core.StateMachine;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Bonus;

public class BonusStateManager(
    BonusBox bonus
) : StateManager<BonusState> {
    public BonusBox Bonus { get; } = bonus;

    public override async Task Init() =>
        await InitState(new None(this));
}

public abstract class BonusState(
    BonusStateManager stateManager
) : State {
    public override BonusStateManager StateManager { get; } = stateManager;
    protected BonusBox Bonus => StateManager.Bonus;
    protected Round Round => Bonus.Round;
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

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (DateTimeOffset.UtcNow >= TimeToSpawn)
            await Bonus.Spawn();
    }
}

public class Spawned(
    BonusStateManager stateManager
) : BonusState(stateManager) {
    public DateTimeOffset SpawnTime { get; private set; }

    public override async Task Start() {
        await base.Start();

        SpawnTime = DateTimeOffset.UtcNow;
        await Round.Players.Share(Bonus.Entity!);
    }
}
