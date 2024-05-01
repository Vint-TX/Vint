using Vint.Core.Battles.Player;
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
    public override async Task Tick() {
        if (Battle.Timer <= 5) {
            foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattleAsTank)) {
                BattleTank tank = battlePlayer.Tank!;

                tank.Disable(true);
                tank.Tank.RemoveComponentIfPresent(tank.StateManager.CurrentState.StateComponent);
            }

            StateManager.SetState(new PreparingToFight(StateManager));
        }

        await base.Tick();
    }
}

public class PreparingToFight(
    WarmUpStateManager stateManager
) : WarmUpState(stateManager) {
    public override async Task Tick() {
        if (Battle.Timer <= 0) {
            foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattleAsTank)) {
                TankStateManager tankStateManager = battlePlayer.Tank!.StateManager;
                tankStateManager.SetState(new Spawn(tankStateManager));
            }

            Battle.Timer = 1;
            StateManager.SetState(new Respawning(StateManager));
        }

        await base.Tick();
    }
}

public class Respawning(
    WarmUpStateManager stateManager
) : WarmUpState(stateManager) {
    public override async Task Tick() {
        if (Battle.Timer <= 0)
            BattleStateManager.SetState(new Running(BattleStateManager));

        await base.Tick();
    }
}
