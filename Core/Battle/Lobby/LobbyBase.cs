using System.Collections.Concurrent;
using System.Collections.Frozen;
using Serilog;
using Vint.Core.Battle.Autopilot;
using Vint.Core.Battle.Autopilot.Connection;
using Vint.Core.Battle.Chat.Templates;
using Vint.Core.Battle.Lobby.Components;
using Vint.Core.Battle.Lobby.State;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Player.User.Components;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Rounds.Components;
using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Logging;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Squads;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Lobby;

public delegate void PlayerRemoved(LobbyBase lobby);

public abstract class LobbyBase : IDisposable {
    protected LobbyBase(QuestManager questManager, BotBuilder botBuilder) {
        Logger = Log.Logger.ForType(GetType());
        QuestManager = questManager;
        BotBuilder = botBuilder;

        TeamHandler = new LobbyTeamHandler(this);
        StateManager = new LobbyStateManager(this);
        ModeHandlerBuilder = new ModeHandlerBuilder(this);
    }

    protected ILogger Logger { get; }

    ConcurrentDictionary<Guid, LobbyPlayer> PlayersDict { get; } = [];
    public ICollection<LobbyPlayer> Players => PlayersDict.Values;
    public IEnumerable<LobbyPlayer> Humans => Players.Where(player => !player.Connection.IsBot);
    public IEnumerable<LobbyPlayer> Bots => Players.Where(player => player.Connection.IsBot);

    public abstract BattleProperties Properties { get; protected set; }
    public LobbyStateManager StateManager { get; }
    public LobbyTeamHandler TeamHandler { get; }

    public abstract IEntity Entity { get; }
    public IEntity ChatEntity { get; } = new BattleLobbyChatTemplate().Create();

    public required PlayerRemoved PlayerRemoved { private get; init; }

    protected Round? Round { get; set; }
    BotBuilder BotBuilder { get; }
    QuestManager QuestManager { get; }
    ModeHandlerBuilder ModeHandlerBuilder { get; }

    public virtual async Task Init() {
        TeamHandler.Init();
        await StateManager.Init();
    }

    public abstract Task Start();

    protected abstract Task PlayerJoined(LobbyPlayer player);

    protected abstract Task PlayerExited(LobbyPlayer player);

    protected abstract Task RemovedFromRound(Tanker tanker);

    protected abstract Task RoundEnded();

    public abstract Task PlayerReady(LobbyPlayer player);

    public async Task AddPlayer(IPlayerConnection connection) {
        if (Players.Count >= Properties.GetValue(BattleProperty.MaxPlayers)) {
            bool success = await TryRemoveBots(1, true);
            if (!success) return;
        }

        connection.Logger.Information("Joining lobby {Id}", Entity.Id);

        Preset preset = connection.Player.CurrentPreset;
        IEntity user = connection.UserContainer.Entity;

        LobbyPlayer player = new(connection, this);
        connection.LobbyPlayer = player;

        await connection.Share(Entity, ChatEntity);

        foreach (LobbyPlayer otherPlayer in Players) {
            await otherPlayer.Connection.UserContainer.ShareTo(connection);
            await connection.UserContainer.ShareTo(otherPlayer.Connection);
        }

        await TeamHandler.ChooseAndSetTeamFor(player);

        await user.AddGroupComponent<BattleLobbyGroupComponent>(Entity);
        await user.AddComponent(new UserEquipmentComponent(preset.Weapon, preset.Hull));

        PlayersDict[player.Id] = player;
        await PlayerJoined(player);
    }

    public async Task AddSquad(Squad squad) {
        List<IPlayerConnection> members = squad.Members.ToList();
        int exceed = Players.Count + members.Count - Properties.GetValue(BattleProperty.MaxPlayers);

        if (exceed > 0) {
            bool success = await TryRemoveBots(exceed, true);
            if (!success) return;
        }

        FrozenDictionary<long, IEntity?> teams = TeamHandler.CalculateTeamsForSquad(members);

        foreach (IPlayerConnection connection in members) {
            connection.Logger.Information("Joining lobby {Id}", Entity.Id);

            Preset preset = connection.Player.CurrentPreset;
            IEntity user = connection.UserContainer.Entity;

            LobbyPlayer player = new(connection, this);
            connection.LobbyPlayer = player;

            await connection.Share(Entity, ChatEntity);

            foreach (LobbyPlayer otherPlayer in Players) {
                await otherPlayer.Connection.UserContainer.ShareTo(connection);
                await connection.UserContainer.ShareTo(otherPlayer.Connection);
            }

            await player.SetTeam(teams[connection.UserContainer.Id]);

            await user.AddGroupComponent<BattleLobbyGroupComponent>(Entity);
            await user.AddComponent(new UserEquipmentComponent(preset.Weapon, preset.Hull));

            PlayersDict[player.Id] = player;
            await PlayerJoined(player);
        }
    }

    public async Task RemovePlayer(LobbyPlayer player) {
        if (StateManager.CurrentState is Starting)
            return;

        IPlayerConnection connection = player.Connection;
        IEntity user = connection.UserContainer.Entity;

        if (!PlayersDict.TryRemove(player.Id, out _)) return;

        connection.Logger.Information("Exited lobby {Id}", Entity.Id);

        await user.RemoveComponent<UserEquipmentComponent>();
        await user.RemoveComponent<BattleLobbyGroupComponent>();
        await user.RemoveComponent<TeamColorComponent>();

        await player.SetReady(false);

        foreach (LobbyPlayer otherPlayer in Players) {
            await connection.UserContainer.UnshareFrom(otherPlayer.Connection);
            await otherPlayer.Connection.UserContainer.UnshareFrom(connection);
        }

        await connection.Unshare(ChatEntity, Entity);
        connection.LobbyPlayer = null;

        await PlayerExited(player);

        PlayerRemoved(this);
        player.Dispose();

        if (!Humans.Any()) {
            foreach (LobbyPlayer p in Players) {
                if (p.InRound)
                    await p.Round.RemoveTanker(p.Tanker);

                await RemovePlayer(p);
            }
        }
    }

    public virtual async Task Tick(TimeSpan deltaTime, CancellationToken cancellationToken) {
        await StateManager.Tick(deltaTime);

        if (Round != null)
            await Round.Tick(deltaTime, cancellationToken);
    }

    public async Task<bool> TryRemoveBots(int count, bool fromWeakerTeam) {
        if (count <= 0) return true;

        List<LobbyPlayer> allBots = Bots.ToList();
        if (allBots.Count < count) return false;

        List<LobbyPlayer> botsToRemove = [];

        if (TeamHandler.IsTeamLobby) {
            // For team battles, prioritize bots from weaker/stronger team
            List<LobbyPlayer> redBots = allBots.Where(bot => bot.TeamColor == TeamColor.Red).ToList();
            List<LobbyPlayer> blueBots = allBots.Where(bot => bot.TeamColor == TeamColor.Blue).ToList();

            int redHumans = TeamHandler.RedHumans.Count();
            int blueHumans = TeamHandler.BlueHumans.Count();

            List<LobbyPlayer> targetTeamBots;
            List<LobbyPlayer> otherTeamBots;

            if (redHumans == blueHumans) {
                // Teams are balanced, no preference
                targetTeamBots = redBots;
                otherTeamBots = blueBots;
            } else if (fromWeakerTeam) {
                // Remove from weaker team (team with fewer humans)
                if (redHumans < blueHumans) {
                    targetTeamBots = redBots;
                    otherTeamBots = blueBots;
                } else {
                    targetTeamBots = blueBots;
                    otherTeamBots = redBots;
                }
            } else {
                // Remove from stronger team (team with more humans)
                if (redHumans > blueHumans) {
                    targetTeamBots = redBots;
                    otherTeamBots = blueBots;
                } else {
                    targetTeamBots = blueBots;
                    otherTeamBots = redBots;
                }
            }

            // Priority order for team battles:
            // 1. Target team bots not in round
            // 2. Target team bots in round
            // 3. Other team bots not in round
            // 4. Other team bots in round
            IEnumerable<LobbyPlayer> prioritizedBots = GetPrioritizedBots(targetTeamBots).Concat(GetPrioritizedBots(otherTeamBots));
            botsToRemove.AddRange(prioritizedBots.Take(count));
        } else {
            // For non-team battles, just prioritize bots not in round
            IEnumerable<LobbyPlayer> prioritizedBots = GetPrioritizedBots(allBots);
            botsToRemove.AddRange(prioritizedBots.Take(count));
        }

        // Remove the selected bots
        if (botsToRemove.Count < count)
            return false;

        await RemoveBots(botsToRemove);
        return true;
    }

    protected async Task RemoveBots(IEnumerable<LobbyPlayer> bots) {
        foreach (LobbyPlayer bot in bots) {
            if (bot.InRound) await bot.Round.RemoveTanker(bot.Tanker);
            else await RemovePlayer(bot);
        }
    }

    protected async Task RemoveAllBots() => await RemoveBots(Bots);

    protected static IEnumerable<LobbyPlayer> GetPrioritizedBots(IReadOnlyCollection<LobbyPlayer> bots) {
        IEnumerable<LobbyPlayer> botsNotInRound = bots.Where(bot => !bot.InRound);
        IOrderedEnumerable<LobbyPlayer> botsInRound = bots
            .Where(bot => bot.InRound)
            .OrderBy(bot => bot.Tanker?.Tank.Entities.RoundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses ?? 0);

        return botsNotInRound.Concat(botsInRound);
    }

    protected async Task AddBots(int count) {
        for (int i = 0; i < count; i++) {
            await AddBot();
        }
    }

    protected async Task AddBot() {
        string nickname = ConfigManager.BotNicknames
            .Except(Players.Select(player => player.Connection.Player.Username))
            .ToList()
            .RandomElement();

        BotConnection bot = await BotBuilder.ConnectNewBot(nickname);

        await bot.CalculateAndSetStatisticsByLobby(this);
        await AddPlayer(bot);
        await PlayerReady(bot.LobbyPlayer!);
    }

    protected async Task<Round> CreateRound() {
        Round round = new(Properties, ModeHandlerBuilder, QuestManager) {
            RoundEnded = RoundEnded,
            TankerRemoved = RemovedFromRound
        };

        await round.Init();
        return round;
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            Round?.Dispose();

            foreach (LobbyPlayer player in Players)
                player.Dispose();

            PlayersDict.Clear();

            TeamHandler.Dispose();
            ChatEntity.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~LobbyBase() => Dispose(false);
}
