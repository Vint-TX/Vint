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
    public override void Tick() {
        if (Battle.Timer <= 4) {
            foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattleAsTank)) {
                BattleTank tank = battlePlayer.Tank!;
                tank.Disable();

                if (tank.Tank.HasComponent(tank.StateManager.CurrentState.StateComponent))
                    tank.Tank.RemoveComponent(tank.StateManager.CurrentState.StateComponent);
            }

            StateManager.SetState(new PreparingToFight(StateManager));
        }

        base.Tick();
    }
}

public class PreparingToFight(
    WarmUpStateManager stateManager
) : WarmUpState(stateManager) {
    public override void Tick() {
        if (Battle.Timer <= 0) {
            foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattleAsTank)) {
                TankStateManager tankStateManager = battlePlayer.Tank!.StateManager;
                tankStateManager.SetState(new Spawn(tankStateManager));
            }

            Battle.Timer = 1;
            StateManager.SetState(new Respawning(StateManager));
        }

        base.Tick();
    }
}

public class Respawning(
    WarmUpStateManager stateManager
) : WarmUpState(stateManager) {
    public override void Tick() {
        if (Battle.Timer <= 0)
            BattleStateManager.SetState(new Running(BattleStateManager));

        base.Tick();
    }
}