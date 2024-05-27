using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using ConcurrentCollections;
using Vint.Core.Battles.ArcadeMode;
using Vint.Core.Battles.Bonus;
using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Type;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.ECS.Templates.Battle.Mode;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Physics;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

public class Battle {
    public Battle() { // Matchmaking battle
        Properties = null!;

        TypeHandler = new MatchmakingHandler(this);
        StateManager = new BattleStateManager(this);

        TypeHandler.Setup();
        Setup();

        LobbyChatEntity = new BattleLobbyChatTemplate().Create();
        BattleChatEntity = new GeneralBattleChatTemplate().Create();
    }

    public Battle(ArcadeModeType arcadeMode) { // Arcade battle
        Properties = null!;

        TypeHandler = new ArcadeHandler(this, arcadeMode);
        StateManager = new BattleStateManager(this);

        TypeHandler.Setup();
        Setup();

        LobbyChatEntity = new BattleLobbyChatTemplate().Create();
        BattleChatEntity = new GeneralBattleChatTemplate().Create();
    }

    public Battle(BattleProperties properties, IPlayerConnection owner) { // Custom battle
        properties.DamageEnabled = true;
        Properties = properties;

        TypeHandler = new CustomHandler(this, owner);
        StateManager = new BattleStateManager(this);

        TypeHandler.Setup();
        Setup();

        LobbyChatEntity = new BattleLobbyChatTemplate().Create();
        BattleChatEntity = new GeneralBattleChatTemplate().Create();
    }

    public long Id => Entity.Id;
    public long LobbyId => LobbyEntity.Id;
    public bool CanAddPlayers => (TypeHandler is CustomHandler { IsOpened: true } ||
                                  StateManager.CurrentState is not Ended) &&
                                 Players.Count(battlePlayer => !battlePlayer.IsSpectator) < Properties.MaxPlayers;
    public bool WasPlayers { get; private set; }
    public double Timer { get; set; }

    public RoundStopTimeComponent? StopTimeComponentBeforeDomination { get; set; }
    public DateTimeOffset? DominationStartTime { get; set; }
    public TimeSpan DominationDuration { get; } = TimeSpan.FromSeconds(30);
    public bool DominationCanBegin => !DominationStartTime.HasValue && Timer > 120 && Timer < Properties.TimeLimit * 60 - 60;

    public BattleStateManager StateManager { get; }
    public BattleProperties Properties { get; set; }
    public MapInfo MapInfo { get; set; }

    public IEntity LobbyEntity { get; set; } = null!;
    public IEntity MapEntity { get; set; } = null!;
    public IEntity RoundEntity { get; private set; } = null!;
    public IEntity Entity { get; private set; } = null!;

    public IEntity LobbyChatEntity { get; }
    public IEntity BattleChatEntity { get; }

    public TypeHandler TypeHandler { get; }
    public ModeHandler ModeHandler { get; private set; } = null!;
    public IDamageProcessor DamageProcessor { get; private set; } = null!;
    public IBonusProcessor? BonusProcessor { get; private set; }
    public Simulation? Simulation { get; private set; }

    public ConcurrentHashSet<BattlePlayer> Players { get; } = [];

    public void Setup() {
        BattleModeTemplate battleModeTemplate = Properties.BattleMode switch {
            BattleMode.DM => new DMTemplate(),
            BattleMode.TDM => new TDMTemplate(),
            BattleMode.CTF => new CTFTemplate(),
            _ => throw new UnreachableException()
        };

        Entity = battleModeTemplate.Create(TypeHandler, LobbyEntity, Properties.ScoreLimit, Properties.TimeLimit * 60, Properties.MaxPlayers, 60);
        RoundEntity = new RoundTemplate().Create(Entity);

        ModeHandler = Properties.BattleMode switch {
            BattleMode.DM => new DMHandler(this),
            BattleMode.TDM => new TDMHandler(this),
            BattleMode.CTF => new CTFHandler(this),
            _ => throw new UnreachableException()
        };

        Properties.DamageEnabled = TypeHandler is not ArcadeHandler { ModeHandler: WithoutDamageHandler };
        DamageProcessor = new DamageProcessor();

        if (Properties.DisabledModules) {
            BonusProcessor = null;
            return;
        }

        IDictionary<BonusType, IEnumerable<Config.MapInformation.Bonus>> bonuses = MapInfo.BonusRegions
            .Get(Properties.BattleMode)
            .ToDictionary();

        BonusProcessor = new BonusProcessor(this, bonuses);
    }

    public async Task UpdateProperties(BattleProperties properties) {
        ModeHandler previousHandler = ModeHandler;

        Properties = properties;
        MapInfo = ConfigManager.MapInfos.Single(map => map.Id == Properties.MapId);
        MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == Properties.MapId);

        await LobbyEntity.RemoveComponent<MapGroupComponent>();
        await LobbyEntity.AddGroupComponent<MapGroupComponent>(MapEntity);

        await LobbyEntity.RemoveComponent<BattleModeComponent>();
        await LobbyEntity.AddComponent(new BattleModeComponent(Properties.BattleMode));

        await LobbyEntity.RemoveComponent<UserLimitComponent>();
        await LobbyEntity.AddComponent(new UserLimitComponent(Properties.MaxPlayers));

        await LobbyEntity.RemoveComponent<GravityComponent>();
        await LobbyEntity.AddComponent(new GravityComponent(Properties.Gravity));

        if (TypeHandler is CustomHandler) {
            await LobbyEntity.RemoveComponent<ClientBattleParamsComponent>();
            await LobbyEntity.AddComponent(new ClientBattleParamsComponent(Properties));
        }

        Setup();
        ModeHandler.TransferParameters(previousHandler);
    }

    public async Task Start() {
        if (ConfigManager.MapNameToTriangles.TryGetValue(MapInfo.Name, out Triangle[]? triangles)) {
            Simulation = Simulation.Create(new BufferPool(), new CollisionCallbacks(), new PoseIntegratorCallbacks(), new SolveDescription(1, 1));
            Simulation.BufferPool.Take(triangles.Length, out Buffer<Triangle> buffer);

            for (int i = 0; i < triangles.Length; i++)
                buffer[i] = triangles[i];

            Simulation.Statics.Add(
                new StaticDescription(Vector3.Zero,
                    Simulation.Shapes.Add(
                        new Mesh(buffer, Vector3.One, Simulation.BufferPool))));
        }

        foreach (BattlePlayer battlePlayer in Players.Where(player => !player.IsSpectator))
            await battlePlayer.Init();

        BonusProcessor?.Start();
    }

    public async Task Finish() {
        if (StateManager.CurrentState is Ended) return;

        await StateManager.SetState(new Ended(StateManager));
        await ModeHandler.OnFinished();

        List<BattlePlayer> players = Players.ToList();
        List<BattlePlayer> tankPlayers = players.Where(player => player.InBattleAsTank).ToList();

        bool hasEnemies = ModeHandler is TeamHandler teamHandler &&
                          teamHandler.BluePlayers.Any() &&
                          teamHandler.RedPlayers.Any() ||
                          tankPlayers.Count > 1;

        foreach (BattlePlayer battlePlayer in players.Where(battlePlayer => battlePlayer.InBattleAsTank)) {
            BattleTank battleTank = battlePlayer.Tank!;
            await battleTank.Disable(true);
            battleTank.CreateUserResult();
        }

        foreach (BattlePlayer battlePlayer in players.Where(battlePlayer => !battlePlayer.InBattle)) {
            try {
                await RemovePlayerFromLobby(battlePlayer);
            } catch { /**/ }
        }

        foreach (BattlePlayer battlePlayer in players.Where(battlePlayer => battlePlayer.InBattle)) {
            try {
                await battlePlayer.OnBattleEnded(hasEnemies);
            } catch { /**/ }
        }

        // todo

        await RoundEntity.AddComponentIfAbsent(new RoundRestartingStateComponent());
        await LobbyEntity.RemoveComponentIfPresent<BattleGroupComponent>();
    }

    public async Task Tick(double deltaTime) {
        Timer -= deltaTime;

        await ModeHandler.Tick();
        await TypeHandler.Tick();
        await StateManager.Tick();

        if (BonusProcessor != null)
            await BonusProcessor.Tick();

        foreach (BattlePlayer battlePlayer in Players)
            await battlePlayer.Tick();
    }

    public async Task AddPlayer(IPlayerConnection connection, bool spectator = false) { // todo squads
        if (connection.InLobby || !spectator && !CanAddPlayers) return;

        connection.Logger.Warning("Joining battle {Id} (spectator: {Bool})", LobbyId, spectator);

        foreach (BattlePlayer battlePlayer in Players.Where(player => !player.IsSpectator))
            await connection.ShareIfUnshared(battlePlayer.PlayerConnection.User);

        if (spectator) {
            connection.BattlePlayer = new BattlePlayer(connection, this, null, true);
        } else {
            Preset preset = connection.Player.CurrentPreset;

            await connection.Share(LobbyEntity, LobbyChatEntity);
            await connection.User.AddGroupComponent<BattleLobbyGroupComponent>(LobbyEntity);
            await connection.User.AddComponent(new UserEquipmentComponent(preset.Weapon.Id, preset.Hull.Id));

            foreach (BattlePlayer battlePlayer in Players)
                await battlePlayer.PlayerConnection.ShareIfUnshared(connection.User);

            connection.BattlePlayer = ModeHandler.SetupBattlePlayer(connection);
            await TypeHandler.PlayerEntered(connection.BattlePlayer);
        }

        Players.Add(connection.BattlePlayer);
        WasPlayers = true;

        if (spectator)
            await connection.BattlePlayer.Init();
    }

    public async Task RemovePlayer(BattlePlayer battlePlayer) { // todo squads
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        IEntity user = connection.User;

        if (battlePlayer.InBattleAsTank)
            await battlePlayer.Tank!.RoundUser.RemoveComponent<RoundUserComponent>();

        await connection.Unshare(Entity, RoundEntity, BattleChatEntity);
        await connection.Unshare(Players
            .Where(player => player.InBattleAsTank &&
                             player != battlePlayer)
            .SelectMany(player => player.Tank!.Entities));

        BonusProcessor?.UnshareEntities(connection);

        if (user.HasComponent<BattleGroupComponent>())
            await user.RemoveComponent<BattleGroupComponent>();
        else
            connection.Logger.Error("User does not have BattleGroupComponent (Battle#RemovePlayer)");

        await ModeHandler.PlayerExited(battlePlayer);

        foreach (Effect effect in Players
                     .Where(player => player.InBattleAsTank)
                     .SelectMany(player => player.Tank!.Effects))
            await effect.Unshare(battlePlayer);

        if (battlePlayer.IsSpectator) {
            await connection.Unshare(battlePlayer.BattleUser);
            await RemovePlayerFromLobby(battlePlayer);
        } else {
            foreach (BattlePlayer player in Players.Where(player => player.InBattle))
                await player.PlayerConnection.Unshare(battlePlayer.Tank!.Entities);

            foreach (BattleModule module in battlePlayer.Tank!.Modules)
                await connection.Unshare(module.SlotEntity, module.Entity);

            battlePlayer.InBattle = false;

            if (TypeHandler is not CustomHandler || !battlePlayer.PlayerConnection.IsOnline)
                await RemovePlayerFromLobby(battlePlayer);

            battlePlayer.Tank = null;

            await ModeHandler.SortPlayers();

            if (!Players.All(player => player.IsSpectator)) return;

            foreach (BattlePlayer spectator in Players.Where(player => player.IsSpectator)) {
                await spectator.PlayerConnection.Send(new KickFromBattleEvent(), spectator.BattleUser);
                await RemovePlayer(spectator);
            }
        }

        await battlePlayer.PlayerConnection.Send(new KickFromBattleEvent(), battlePlayer.BattleUser);
    }

    public async Task RemovePlayerFromLobby(BattlePlayer battlePlayer) {
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        connection.Logger.Warning("Leaving battle {Id}", LobbyId);

        Players.TryRemove(battlePlayer);

        if (battlePlayer.IsSpectator) {
            foreach (BattlePlayer player in Players.Where(player => !player.IsSpectator))
                await connection.Unshare(player.PlayerConnection.User);
        } else {
            IEntity user = connection.User;

            await TypeHandler.PlayerExited(battlePlayer);
            await ModeHandler.RemoveBattlePlayer(battlePlayer);

            await user.RemoveComponent<UserEquipmentComponent>();
            await user.RemoveComponent<BattleLobbyGroupComponent>();
            await user.RemoveComponentIfPresent<MatchMakingUserReadyComponent>();
            await connection.Unshare(LobbyEntity, LobbyChatEntity);

            foreach (BattlePlayer player in Players) {
                await player.PlayerConnection.Unshare(user);
                await connection.UnshareIfShared(player.PlayerConnection.User);
            }

            if (Players.All(player => player.IsSpectator)) {
                foreach (BattlePlayer spectator in Players) {
                    await spectator.PlayerConnection.Send(new KickFromBattleEvent(), spectator.BattleUser);
                    await RemovePlayer(spectator);
                }
            }
        }

        connection.BattlePlayer = null;
    }

    public bool IsUnfair() =>
        Players.Count <= 3 ||
        ModeHandler is TeamHandler teamHandler &&
        Math.Abs(teamHandler.RedPlayers.Count() - teamHandler.BluePlayers.Count()) >= 2;

    public override int GetHashCode() => LobbyId.GetHashCode();
}
