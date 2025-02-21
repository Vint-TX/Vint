using System.Collections.Concurrent;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Lobby;

public delegate void PlayerRemoved(LobbyBase lobby);

public abstract class LobbyBase : IDisposable {
    protected LobbyBase(QuestManager questManager) {
        QuestManager = questManager;

        TeamHandler = new LobbyTeamHandler(this);
        StateManager = new LobbyStateManager(this);
        ModeHandlerBuilder = new ModeHandlerBuilder(this);
    }

    ConcurrentDictionary<Guid, LobbyPlayer> PlayersDict { get; } = [];
    public ICollection<LobbyPlayer> Players => PlayersDict.Values;

    public abstract BattleProperties Properties { get; protected set; }
    public LobbyStateManager StateManager { get; }
    public LobbyTeamHandler TeamHandler { get; }

    public abstract IEntity Entity { get; }
    public IEntity ChatEntity { get; } = new BattleLobbyChatTemplate().Create();

    public required PlayerRemoved PlayerRemoved { private get; init; }

    protected Round? Round { get; set; }
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
        if (Players.Count >= Properties.MaxPlayers)
            return;

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
    }

    public virtual async Task Tick(TimeSpan deltaTime) {
        await StateManager.Tick(deltaTime);

        if (Round != null)
            await Round.Tick(deltaTime);
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
        if (disposing) { // todo dispose entities
            Round?.Dispose();

            foreach (LobbyPlayer player in Players)
                player.Dispose();

            PlayersDict.Clear();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~LobbyBase() => Dispose(false);
}
