using System.Numerics;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Flag;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Flag;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Flags;

public abstract class FlagState(
    FlagStateManager stateManager
) : State {
    public override FlagStateManager StateManager { get; } = stateManager;
    protected Flag Flag => StateManager.Flag;
    protected Battle Battle => Flag.Battle;
}

public class OnPedestal(
    FlagStateManager stateManager
) : FlagState(stateManager);

public class Captured(
    FlagStateManager stateManager,
    IEntity carrierTank
) : FlagState(stateManager) {
    public override async Task Start() {
        await base.Start();
        await Flag.Entity.AddGroupComponent<TankGroupComponent>(carrierTank);

        foreach (BattlePlayer battlePlayer in Battle.Players)
            await battlePlayer.PlayerConnection.Send(new FlagPickupEvent(), Flag.Entity);

        await Flag.Entity.RemoveComponentIfPresent<FlagHomeStateComponent>();
        await Flag.Entity.RemoveComponentIfPresent<FlagGroundedStateComponent>();
    }
}

public class OnGround(
    FlagStateManager stateManager,
    Vector3 position,
    bool isUserAction
) : FlagState(stateManager) {
    DateTimeOffset ReturnAtTime { get; } = DateTimeOffset.UtcNow.AddMinutes(1);

    public override async Task Start() {
        await base.Start();

        foreach (BattlePlayer battlePlayer in Battle.Players)
            await battlePlayer.PlayerConnection.Send(new FlagDropEvent(isUserAction), Flag.Entity);

        await Flag.Entity.RemoveComponent<TankGroupComponent>();
        await Flag.Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = position);
        await Flag.Entity.AddComponent<FlagGroundedStateComponent>();
    }

    public override async Task Tick() {
        await base.Tick();

        if (DateTimeOffset.UtcNow < ReturnAtTime) return;

        await Flag.Return();
    }
}
