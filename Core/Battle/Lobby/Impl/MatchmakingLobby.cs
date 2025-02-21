using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.Quests;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Lobby.Impl;

public abstract class MatchmakingLobby(
    QuestManager questManager
) : LobbyBase(questManager) {
    public override async Task Start() {
        if (StateManager.CurrentState is not Countdown)
            return;

        await StateManager.SetState(new MatchmakingLobbyStarting(StateManager));

        Round = await CreateRound();
        await Entity.AddGroupComponent<BattleGroupComponent>(Round.Entity);

        await StateManager.SetState(new Running(StateManager, Round));

        foreach (LobbyPlayer player in Players) {
            await player.SetRoundJoinTime(DateTimeOffset.UtcNow);
            await Round.AddTanker(player);
        }
    }

    protected override async Task PlayerJoined(LobbyPlayer player) {
        IPlayerConnection connection = player.Connection;
        IEntity user = connection.UserContainer.Entity;

        await user.AddComponent<MatchMakingUserComponent>();

        if (StateManager.CurrentState is Awaiting && CanStartOrKeepCountdown()) {
            DateTimeOffset startTime = DateTimeOffset.UtcNow.AddSeconds(20);
            await StateManager.SetState(new Countdown(StateManager, startTime));
            return;
        }

        if (StateManager.CurrentState is not Running) return;

        await player.SetRoundJoinTime(DateTimeOffset.UtcNow.AddSeconds(20));
    }

    protected override async Task PlayerExited(LobbyPlayer player) {
        IPlayerConnection connection = player.Connection;
        IEntity user = connection.UserContainer.Entity;

        await user.RemoveComponent<MatchMakingUserComponent>();

        if (StateManager.CurrentState is Countdown && !CanStartOrKeepCountdown()) {
            await StateManager.SetState(new Awaiting(StateManager));
            return;
        }

        if (StateManager.CurrentState is Awaiting or Countdown) return;

        bool roundEnded = StateManager.CurrentState is Ended;
        bool hasEnemies = StateManager.CurrentState is Running { Round.ModeHandler.HasEnemies: true };

        await connection.UpdateDeserterStatus(roundEnded, hasEnemies);

        if (roundEnded)
            await connection.CheckLoginRewards();
    }

    protected override async Task RemovedFromRound(Tanker tanker) =>
        await RemovePlayer(tanker.Connection.LobbyPlayer!);

    protected override async Task RoundEnded() =>
        await StateManager.SetState(new Ended(StateManager));

    public override async Task PlayerReady(LobbyPlayer player) {
        await player.SetReady(true);

        if (StateManager.CurrentState is Running)
            await player.SetRoundJoinTime(DateTimeOffset.UtcNow.AddSeconds(3));
    }

    public override async Task Tick(TimeSpan deltaTime) {
        if (StateManager.CurrentState is Running running) {
            foreach (LobbyPlayer player in Players.Where(player => !player.InRound &&
                                                                   player.RoundJoinTime <= DateTimeOffset.UtcNow))
                await running.Round.AddTanker(player);
        }

        await base.Tick(deltaTime);
    }

    protected virtual bool CanStartOrKeepCountdown() => Players.Count > 0;
}
