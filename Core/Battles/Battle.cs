using System.Diagnostics;
using Serilog;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Type;
using Vint.Core.Battles.Weapons.Damage;
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

    public ILogger Logger { get; } = Log.Logger.ForType(typeof(Battle));

    public long Id => Entity.Id;
    public long LobbyId => LobbyEntity.Id;
    public bool CanAddPlayers => Players.Count(battlePlayer => !battlePlayer.IsSpectator) < Properties.MaxPlayers;
    public bool WasPlayers { get; private set; }
    public double Timer { get; set; }

    public BattleStateManager StateManager { get; }
    public BattleProperties Properties { get; set; }
    public MapInfo MapInfo { get; set; } = null!;

    public IEntity LobbyEntity { get; set; } = null!;
    public IEntity MapEntity { get; set; } = null!;
    public IEntity RoundEntity { get; private set; } = null!;
    public IEntity Entity { get; private set; } = null!;

    public IEntity LobbyChatEntity { get; }
    public IEntity BattleChatEntity { get; }

    public TypeHandler TypeHandler { get; }
    public ModeHandler ModeHandler { get; private set; } = null!;
    public IDamageProcessor DamageProcessor { get; private set; } = null!;

    public HashSet<BattlePlayer> Players { get; } = [];

    public void Setup() {
        BattleModeTemplate battleModeTemplate = Properties.BattleMode switch {
            BattleMode.DM => new DMTemplate(),
            BattleMode.TDM => new TDMTemplate(),
            BattleMode.CTF => new CTFTemplate(),
            _ => throw new UnreachableException()
        };

        Entity = battleModeTemplate.Create(TypeHandler, LobbyEntity, Properties.ScoreLimit, Properties.TimeLimit * 60, 60);
        RoundEntity = new RoundTemplate().Create(Entity);

        // todo height maps (or server physics)

        ModeHandler = Properties.BattleMode switch {
            BattleMode.DM => new DMHandler(this),
            BattleMode.TDM => new TDMHandler(this),
            BattleMode.CTF => new CTFHandler(this),
            _ => throw new UnreachableException()
        };

        DamageProcessor = new DamageProcessor(this);
    }

    public void UpdateProperties(BattleProperties properties) {
        ModeHandler previousHandler = ModeHandler;

        Properties = properties;
        MapInfo = ConfigManager.MapInfos.Values.Single(map => map.Id == Properties.MapId);
        MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == Properties.MapId);

        LobbyEntity.RemoveComponent<MapGroupComponent>();
        LobbyEntity.AddComponent(new MapGroupComponent(MapEntity));

        LobbyEntity.RemoveComponent<BattleModeComponent>();
        LobbyEntity.AddComponent(new BattleModeComponent(Properties.BattleMode));

        LobbyEntity.RemoveComponent<UserLimitComponent>();
        LobbyEntity.AddComponent(new UserLimitComponent(Properties.MaxPlayers));

        LobbyEntity.RemoveComponent<GravityComponent>();
        LobbyEntity.AddComponent(new GravityComponent(Properties.Gravity));

        if (TypeHandler is CustomHandler) {
            LobbyEntity.RemoveComponent<ClientBattleParamsComponent>();
            LobbyEntity.AddComponent(new ClientBattleParamsComponent(Properties));
        }

        Setup();
        ModeHandler.TransferParameters(previousHandler);
    }

    public void Start() {
        // todo modules

        // todo teams

        foreach (BattlePlayer battlePlayer in Players.ToList().Where(player => !player.IsSpectator))
            battlePlayer.Init();
    }

    public void Finish() {
        StateManager.SetState(new Ended(StateManager));
        ModeHandler.OnFinished();

        List<BattlePlayer> players = Players.ToList();

        foreach (BattlePlayer battlePlayer in players.Where(battlePlayer => battlePlayer.InBattleAsTank)) {
            BattleTank battleTank = battlePlayer.Tank!;
            battleTank.Disable(true);
        }

        foreach (BattlePlayer battlePlayer in players)
            battlePlayer.OnBattleEnded();

        // todo

        RoundEntity.AddComponentIfAbsent(new RoundRestartingStateComponent());
        LobbyEntity.RemoveComponentIfPresent<BattleGroupComponent>();
    }

    public void Tick(double deltaTime) {
        Timer -= deltaTime;

        ModeHandler.Tick();
        TypeHandler.Tick();
        StateManager.Tick();

        foreach (BattlePlayer battlePlayer in Players.ToList())
            battlePlayer.Tick();
    }

    public void AddPlayer(IPlayerConnection connection, bool spectator = false) { // todo squads
        if (connection.InLobby || !spectator && !CanAddPlayers) return;

        connection.Logger.Warning("Joining battle {Id} (spectator: {Bool})", LobbyId, spectator);

        foreach (BattlePlayer battlePlayer in Players.Where(player => !player.IsSpectator))
            connection.ShareIfUnshared(battlePlayer.PlayerConnection.User);

        if (spectator) {
            connection.BattlePlayer = new BattlePlayer(connection, this, null, true);
        } else {
            Preset preset = connection.Player.CurrentPreset;

            connection.Share(LobbyEntity, LobbyChatEntity);
            connection.User.AddComponent(new BattleLobbyGroupComponent(LobbyEntity));
            connection.User.AddComponent(new UserEquipmentComponent(preset.Weapon.Id, preset.Hull.Id));

            foreach (BattlePlayer battlePlayer in Players.Where(player => !player.IsSpectator))
                battlePlayer.PlayerConnection.ShareIfUnshared(connection.User);

            connection.BattlePlayer = ModeHandler.SetupBattlePlayer(connection);
            TypeHandler.PlayerEntered(connection.BattlePlayer);
        }

        Players.Add(connection.BattlePlayer);
        WasPlayers = true;

        if (spectator)
            connection.BattlePlayer.Init();
    }

    public void RemovePlayer(BattlePlayer battlePlayer) { // todo squads
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        IEntity user = connection.User;

        connection.Unshare(Entity, RoundEntity, BattleChatEntity);
        connection.Unshare(Players
            .Where(player => player.InBattleAsTank &&
                             player != battlePlayer)
            .SelectMany(player => player.Tank!.Entities));

        user.RemoveComponent<BattleGroupComponent>();
        ModeHandler.PlayerExited(battlePlayer);

        if (battlePlayer.IsSpectator) {
            connection.Unshare(battlePlayer.BattleUser);
            RemovePlayerFromLobby(battlePlayer);
        } else {
            foreach (BattlePlayer player in Players.Where(player => player.InBattle))
                player.PlayerConnection.Unshare(battlePlayer.Tank!.Entities);

            battlePlayer.InBattle = false;
            battlePlayer.Tank = null;

            if (TypeHandler is not CustomHandler || !battlePlayer.PlayerConnection.IsOnline)
                RemovePlayerFromLobby(battlePlayer);

            ModeHandler.SortPlayers();

            if (Players.Any(player => !player.IsSpectator)) return;

            foreach (BattlePlayer spectator in Players.ToList()) {
                spectator.PlayerConnection.Send(new KickFromBattleEvent(), spectator.BattleUser);
                RemovePlayer(spectator);
            }
        }
    }

    public void RemovePlayerFromLobby(BattlePlayer battlePlayer) {
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        connection.Logger.Warning("Leaving battle {Id}", LobbyId);

        Players.Remove(battlePlayer);

        if (battlePlayer.IsSpectator) {
            foreach (BattlePlayer player in Players.Where(player => !player.IsSpectator))
                connection.Unshare(player.PlayerConnection.User);
        } else {
            IEntity user = connection.User;

            TypeHandler.PlayerExited(battlePlayer);
            ModeHandler.RemoveBattlePlayer(battlePlayer);

            user.RemoveComponent<UserEquipmentComponent>();
            user.RemoveComponent<BattleLobbyGroupComponent>();
            user.RemoveComponentIfPresent<MatchMakingUserReadyComponent>();
            connection.Unshare(LobbyEntity, LobbyChatEntity);

            foreach (BattlePlayer player in Players.Where(player => !player.IsSpectator)) {
                player.PlayerConnection.Unshare(user);
                connection.Unshare(player.PlayerConnection.User);
            }

            if (Players.All(player => player.IsSpectator)) {
                foreach (BattlePlayer spectator in Players.ToList()) {
                    spectator.PlayerConnection.Send(new KickFromBattleEvent(), spectator.BattleUser);
                    RemovePlayer(spectator);
                }
            }
        }

        connection.BattlePlayer = null;
    }

    public override int GetHashCode() => LobbyId.GetHashCode();
}