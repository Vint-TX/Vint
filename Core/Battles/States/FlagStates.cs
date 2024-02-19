using System.Numerics;
using Vint.Core.Battles.Flags;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Flag;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Flag;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public abstract class FlagState(
    FlagStateManager stateManager
) : State {
    public override FlagStateManager StateManager { get; } = stateManager;
    protected Flag Flag { get; } = stateManager.Flag;
    protected Battle Battle { get; } = stateManager.Flag.Battle;
}

public class OnPedestal(
    FlagStateManager stateManager
) : FlagState(stateManager);

public class Captured(
    FlagStateManager stateManager,
    IEntity carrierTank
) : FlagState(stateManager) {
    public override void Start() {
        base.Start();
        Flag.Entity.AddComponent(new TankGroupComponent(carrierTank));

        foreach (BattlePlayer battlePlayer in Battle.Players)
            battlePlayer.PlayerConnection.Send(new FlagPickupEvent(), Flag.Entity);

        Flag.Entity.RemoveComponentIfPresent<FlagHomeStateComponent>();
        Flag.Entity.RemoveComponentIfPresent<FlagGroundedStateComponent>();
    }
}

public class OnGround(
    FlagStateManager stateManager,
    bool isUserAction
) : FlagState(stateManager) {
    DateTimeOffset ReturnAtTime { get; } = DateTimeOffset.UtcNow.AddMinutes(1);

    public override void Start() {
        base.Start();

        foreach (BattlePlayer battlePlayer in Battle.Players)
            battlePlayer.PlayerConnection.Send(new FlagDropEvent(isUserAction), Flag.Entity);

        Vector3 newPosition = Flag.Carrier!.Tank!.Position - Vector3.UnitY;

        Flag.Entity.RemoveComponent<TankGroupComponent>();
        Flag.Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = newPosition);
        Flag.Entity.AddComponent(new FlagGroundedStateComponent());
    }

    public override void Tick() {
        base.Tick();

        if (DateTimeOffset.UtcNow < ReturnAtTime) return;

        Flag.Return();
    }
}