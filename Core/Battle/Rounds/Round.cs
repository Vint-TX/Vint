using System.Collections.Concurrent;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Vint.Core.Battle.Bonus;
using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Damage.Processor;
using Vint.Core.Battle.Mines;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Physics;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Rounds;

public class Round : IDisposable {
    public Round(
        BattleProperties properties,
        ModeHandlerBuilder modeHandlerBuilder,
        QuestManager questManager
    ) {
        QuestManager = questManager;
        Properties = properties;

        StateManager = new RoundStateManager(this, sm => Properties.WarmUpDuration > TimeSpan.Zero ? new WarmUp(sm) : new Running(sm));
        Simulation = CreateSimulation(Properties.MapInfo);

        WarmUpStartTime = DateTimeOffset.UtcNow;
        WarmUpEndTime = StartTime = WarmUpStartTime + Properties.WarmUpDuration;
        EndTime = StartTime + TimeSpan.FromMinutes(Properties.TimeLimit);

        Entity = new RoundTemplate().Create(EndTime);
        ModeHandler = modeHandlerBuilder.BuildModeHandler(this);
        ChatEntity = new GeneralBattleChatTemplate().Create();

        if (!Properties.DisabledModules)
            BonusProcessor = new BonusProcessor(this);

        DamageCalculator = Properties.DamageEnabled
            ? new DamageCalculator()
            : new ZeroDamageCalculator();
    }

    public BattleProperties Properties { get; }

    ConcurrentDictionary<Guid, BattlePlayer> PlayersDict { get; } = [];

    public ICollection<BattlePlayer> Players => PlayersDict.Values;
    public IEnumerable<Tanker> Tankers => Players.OfType<Tanker>();
    public IEnumerable<Spectator> Spectators => Players.OfType<Spectator>();

    public DateTimeOffset StartTime { get; }
    public DateTimeOffset EndTime { get; }
    public TimeSpan Remaining => EndTime - DateTimeOffset.UtcNow;
    public TimeSpan Elapsed => DateTimeOffset.UtcNow - StartTime;

    public DateTimeOffset WarmUpStartTime { get; }
    public DateTimeOffset WarmUpEndTime { get; }
    public TimeSpan RemainingWarmUp => WarmUpEndTime - DateTimeOffset.UtcNow;
    public TimeSpan ElapsedWarmUp => DateTimeOffset.UtcNow - WarmUpStartTime;

    public RoundStateManager StateManager { get; }
    public Simulation Simulation { get; }

    public ModeHandler ModeHandler { get; }

    public IEntity Entity { get; }
    public IEntity ChatEntity { get; }

    public IBonusProcessor? BonusProcessor { get; }
    public IDamageCalculator DamageCalculator { get; }
    public IDamageProcessor DamageProcessor { get; } = new DamageProcessor();
    public MineProcessor MineProcessor { get; } = new();

    public required RoundEnded RoundEnded { private get; init; }
    public required TankerRemoved TankerRemoved { private get; init; }

    QuestManager QuestManager { get; }

    public async Task Init() {
        await ModeHandler.Init();
        await StateManager.Init();

        if (BonusProcessor != null)
            await BonusProcessor.Init();
    }

    public async Task End() {
        if (StateManager.CurrentState is Ended)
            return;

        Ended ended = new(StateManager);
        await StateManager.SetState(ended);

        await ModeHandler.OnRoundEnded();

        foreach (Tanker tanker in Tankers) {
            await tanker.Tank.ResetAndDisable();
            tanker.Tank.CreateUserResult();
        }

        foreach (BattlePlayer player in Players)
            await player.OnRoundEnded(ended.WasEnemies, QuestManager);

        await Entity.AddComponentIfAbsent(new RoundRestartingStateComponent());

        await RoundEnded();
    }

    public async Task AddTanker(LobbyPlayer player) {
        int tankersCount = Tankers.Count();

        if (StateManager.CurrentState is Ended || tankersCount >= Properties.MaxPlayers)
            return;

        await player.JoinRound(this);
        Tanker tanker = player.Tanker;

        await AddPlayerCommon(tanker);
        BonusProcessor?.GoldProcessor.PlayersCountChanged(tankersCount + 1);

        await tanker.Share(tanker.Tank.Entities); // share self entities after AddPlayerCommon because ModeHandler needs to share own entities first
    }

    public async Task AddSpectator(IPlayerConnection connection) {
        if (StateManager.CurrentState is Ended)
            return;

        Spectator spectator = new(connection, this);
        await spectator.Init();
        await AddPlayerCommon(spectator);
    }

    public async Task RemoveTanker(Tanker tanker) {
        await RemovePlayerCommon(tanker);

        int count = Tankers.Count();

        BonusProcessor?.GoldProcessor.PlayersCountChanged(count);
        await ModeHandler.PlayerExited(tanker);

        await TankerRemoved(tanker);

        if (count <= 0)
            await End();
    }

    public async Task RemoveSpectator(Spectator spectator) {
        await RemovePlayerCommon(spectator);
        spectator.Dispose();
    }

    async Task AddPlayerCommon(BattlePlayer player) {
        PlayersDict[player.Id] = player;
        await ModeHandler.PlayerJoined(player);
    }

    async Task RemovePlayerCommon(BattlePlayer player) {
        await player.DeInit();
        PlayersDict.TryRemove(player.Id, out _);
    }

    public async Task Tick(TimeSpan deltaTime) {
        if (StateManager.CurrentState is Ended)
            return;

        await ModeHandler.Tick(deltaTime);
        await StateManager.Tick(deltaTime);

        if (BonusProcessor != null)
            await BonusProcessor.Tick(deltaTime);

        foreach (BattlePlayer player in Players)
            await player.Tick(deltaTime);
    }

    static Simulation CreateSimulation(MapInfo mapInfo) {
        Triangle[] triangles = mapInfo.Triangles.Value;

        Simulation simulation =
            Simulation.Create(new BufferPool(), new CollisionCallbacks(), new PoseIntegratorCallbacks(), new SolveDescription(1, 1));
        simulation.BufferPool.Take(triangles.Length, out Buffer<Triangle> buffer);

        buffer.CopyFrom(triangles, 0, 0, triangles.Length);

        Mesh map = new(buffer, Vector3.One, simulation.BufferPool);
        StaticDescription mapDescription = new(Vector3.Zero, simulation.Shapes.Add(map));
        simulation.Statics.Add(mapDescription);

        return simulation;
    }

    public bool IsUnfair() => Tankers.Count() < 4 ||
                              (ModeHandler is TeamHandler teamHandler &&
                              teamHandler.IsUnfair());

    void Dispose(bool disposing) {
        if (disposing) { // todo dispose entities
            Simulation.Dispose();
            ModeHandler.Dispose();

            foreach (BattlePlayer player in Players)
                player.Dispose();

            PlayersDict.Clear();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Round() => Dispose(false);
}

public delegate Task RoundEnded();

public delegate Task TankerRemoved(Tanker tanker);
