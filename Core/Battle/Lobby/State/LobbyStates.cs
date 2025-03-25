using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.StateMachine;

namespace Vint.Core.Battle.Lobby;

public class LobbyStateManager(
    LobbyBase lobby
) : StateManager<LobbyState> {
    public LobbyBase Lobby { get; } = lobby;

    public override async Task Init() =>
        await InitState(new Awaiting(this));
}

public abstract class LobbyState(
    LobbyStateManager stateManager
) : State {
    public override LobbyStateManager StateManager { get; } = stateManager;
    protected LobbyBase Lobby => StateManager.Lobby;
}

public abstract class Starting(
    LobbyStateManager stateManager
) : LobbyState(stateManager);

public abstract class Starting<TComponent>(
    LobbyStateManager stateManager
) : Starting(stateManager) where TComponent : class, IComponent {
    protected abstract TComponent StateComponent { get; }

    public override async Task Start() {
        await base.Start();
        await Lobby.Entity.AddComponent(StateComponent);
    }

    public override async Task Finish() {
        await base.Finish();
        await Lobby.Entity.RemoveComponent<TComponent>();
    }
}

public class Awaiting(
    LobbyStateManager stateManager
) : LobbyState(stateManager);

public class Countdown(
    LobbyStateManager stateManager,
    DateTimeOffset startTime
) : LobbyState(stateManager) {
    public override async Task Start() {
        await base.Start();
        await Lobby.Entity.AddComponent(new MatchmakingLobbyStartTimeComponent(startTime));
    }

    public override async Task Finish() {
        await base.Finish();
        await Lobby.Entity.RemoveComponent<MatchmakingLobbyStartTimeComponent>();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (DateTimeOffset.UtcNow >= startTime)
            await Lobby.Start();
    }
}

public class CustomLobbyStarting(
    LobbyStateManager stateManager
) : Starting<LobbyStartingStateComponent>(stateManager) {
    protected override LobbyStartingStateComponent StateComponent { get; } = new(DateTimeOffset.UtcNow);
}

public class MatchmakingLobbyStarting(
    LobbyStateManager stateManager
) : Starting<MatchmakingLobbyStartingComponent>(stateManager) {
    protected override MatchmakingLobbyStartingComponent StateComponent { get; } = new();
}

public class Running(
    LobbyStateManager stateManager,
    Round round
) : LobbyState(stateManager) {
    public Round Round { get; } = round;
}

public class Ended(
    LobbyStateManager stateManager
) : LobbyState(stateManager);
