using Vint.Core.Battles.Tank;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public abstract class WarmUpState(
    WarmUpStateManager stateManager
) : State {
    public override WarmUpStateManager StateManager { get; } = stateManager;
    protected BattleStateManager BattleStateManager => StateManager.BattleStateManager;
    protected Battle Battle => BattleStateManager.Battle;
}

public class WarmingUp(
    WarmUpStateManager stateManager
) : WarmUpState(stateManager) {
    public override async Task Tick(TimeSpan deltaTime) {
        if (Battle.Timer.TotalSeconds <= 5) {
            foreach (BattleTank tank in Battle.Players.Where(player => player.InBattleAsTank).Select(player => player.Tank!)) {
                await tank.Disable(true);
            }

            await StateManager.SetState(new PreparingToFight(StateManager));
        }

        await base.Tick(deltaTime);
    }
}

public class PreparingToFight(
    WarmUpStateManager stateManager
) : WarmUpState(stateManager) {
    public override async Task Tick(TimeSpan deltaTime) {
        if (Battle.Timer <= TimeSpan.Zero) {
            foreach (BattleTank tank in Battle.Players.Where(player => player.InBattleAsTank).Select(player => player.Tank!)) {
                TankStateManager tankStateManager = tank.StateManager;
                await tankStateManager.SetState(new Spawn(tankStateManager));
            }

            await StateManager.SetState(new Respawning(StateManager));
        }

        await base.Tick(deltaTime);
    }
}

public class Respawning(
    WarmUpStateManager stateManager
) : WarmUpState(stateManager) {
    public override async Task Tick(TimeSpan deltaTime) {
        if (Battle.Timer <= TimeSpan.Zero)
            await BattleStateManager.SetState(new Running(BattleStateManager));

        await base.Tick(deltaTime);
    }
}
