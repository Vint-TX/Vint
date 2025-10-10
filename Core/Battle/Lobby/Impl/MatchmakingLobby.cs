using Vint.Core.Battle.Autopilot;
using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Lobby.State;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.Matchmaking.Components;
using Vint.Core.Quests;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Battle.Lobby.Impl;

public abstract class MatchmakingLobby(
    QuestManager questManager,
    BotBuilder botBuilder
) : LobbyBase(questManager, botBuilder) {
    const double FillWithBotsScaleFactor = 0.75;
    int FillWithBotsThreshold => (int)Math.Floor(Properties.GetValue(BattleProperty.MaxPlayers) * Math.Pow(FillWithBotsScaleFactor, 2));

    public override async Task Start() {
        if (StateManager.CurrentState is not Countdown)
            return;

        await StateManager.SetState(new MatchmakingLobbyStarting(StateManager));

        Round = await CreateRound();
        await Entity.AddGroupComponent<BattleGroupComponent>(Round.Entity);

        await StateManager.SetState(new Running(StateManager, Round));

        foreach (LobbyPlayer player in Players.OrderBy(player => player.Connection.IsBot)) { // players are added before bots
            await player.SetRoundJoinTime(DateTimeOffset.UtcNow);
            await Round.AddTanker(player);
        }
    }

    protected override async Task PlayerJoined(LobbyPlayer player) {
        IPlayerConnection connection = player.Connection;
        IEntity user = connection.UserContainer.Entity;

        await user.AddComponent<MatchMakingUserComponent>();

        if (!connection.IsBot)
            await UpdateBotsFilling();

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

        if (!connection.IsBot)
            await UpdateBotsFilling();

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

    public override async Task Tick(TimeSpan deltaTime, CancellationToken cancellationToken) {
        if (StateManager.CurrentState is Running running) {
            foreach (LobbyPlayer player in Players.Where(player => !player.InRound &&
                                                                   player.RoundJoinTime <= DateTimeOffset.UtcNow))
                await running.Round.AddTanker(player);
        }

        await base.Tick(deltaTime, cancellationToken);
    }

    protected virtual bool CanStartOrKeepCountdown() => Humans.Any();

    async Task UpdateBotsFilling() {
        try {
            if (StateManager.CurrentState is Ended or Running { Round.Remaining.TotalMinutes: <= 2 })
                return;

            int humanCount = Humans.Count();

            if (humanCount < FillWithBotsThreshold) {
                await FillLobbyWithBots(humanCount);
            } else if (TeamHandler.IsTeamLobby) {
                await BalanceTeamBots();
            } else {
                await RemoveAllBots();
            }
        } catch (Exception e) {
            Logger.Error(e, "Error while updating bots filling");
        }
    }

    async Task FillLobbyWithBots(int humanCount) {
        int maxPlayers = Properties.GetValue(BattleProperty.MaxPlayers);
        int targetBotCount = maxPlayers - humanCount;
        int currentBotCount = Bots.Count();
        int botsToAdd = Math.Max(0, targetBotCount - currentBotCount);

        for (int i = 0; i < botsToAdd; i++) {
            await AddBot();
        }
    }

    async Task BalanceTeamBots() {
        List<LobbyPlayer> redPlayers = TeamHandler.RedPlayers!.ToList();
        List<LobbyPlayer> bluePlayers = TeamHandler.BluePlayers!.ToList();

        int redHumans = redPlayers.Count(p => !p.Connection.IsBot);
        int blueHumans = bluePlayers.Count(p => !p.Connection.IsBot);

        if (redHumans == blueHumans) {
            await RemoveAllBots();
        } else if (redHumans > blueHumans) {
            await BalanceTeams(redPlayers, bluePlayers, redHumans - blueHumans);
        } else {
            await BalanceTeams(bluePlayers, redPlayers, blueHumans - redHumans);
        }
    }

    async Task BalanceTeams(IEnumerable<LobbyPlayer> strongerTeam, IEnumerable<LobbyPlayer> weakerTeam, int humanDifference) {
        // Remove all bots from the stronger team
        await RemoveBots(strongerTeam.Where(p => p.Connection.IsBot));

        // Balance the weaker team
        List<LobbyPlayer> weakerTeamBots = weakerTeam.Where(p => p.Connection.IsBot).ToList();
        int currentWeakerTeamBots = weakerTeamBots.Count;

        if (currentWeakerTeamBots < humanDifference) {
            await AddBots(humanDifference - currentWeakerTeamBots);
        } else if (currentWeakerTeamBots > humanDifference) {
            int botsToRemove = currentWeakerTeamBots - humanDifference;
            IEnumerable<LobbyPlayer> prioritizedWeakerTeamBots = GetPrioritizedBots(weakerTeamBots);
            await RemoveBots(prioritizedWeakerTeamBots.Take(botsToRemove));
        }
    }
}
