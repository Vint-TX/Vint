using System.Diagnostics;
using Serilog;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Type;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database.Models;
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
        IsCustom = false;
        Properties = default;

        TypeHandler = new MatchmakingHandler(this);
        StateManager = new BattleStateManager(this);

        Setup();
    }

    public Battle(BattleProperties properties, IPlayerConnection owner) { // Custom battle
        IsCustom = true;
        Properties = properties;

        TypeHandler = new CustomHandler(this, owner);
        StateManager = new BattleStateManager(this);

        Setup();
    }

    public ILogger Logger { get; } = Log.Logger.ForType(typeof(Battle));

    public long Id => BattleEntity.Id;
    public long LobbyId => LobbyEntity.Id;
    public bool CanAddPlayers => Players.Count(battlePlayer => !battlePlayer.IsSpectator) < Properties.MaxPlayers;
    public bool IsCustom { get; }
    public bool WasPlayers { get; private set; }
    public double Timer { get; set; }

    public BattleStateManager StateManager { get; }
    public BattleProperties Properties { get; set; }
    public MapInfo MapInfo { get; set; } = null!;

    public IEntity BattleEntity { get; set; } = null!;
    public IEntity LobbyEntity { get; set; } = null!;
    public IEntity RoundEntity { get; set; } = null!;
    public IEntity MapEntity { get; set; } = null!;

    public IEntity LobbyChatEntity { get; private set; } = null!;
    public IEntity BattleChatEntity { get; private set; } = null!;

    public TypeHandler TypeHandler { get; }
    public ModeHandler ModeHandler { get; private set; } = null!;

    public List<BattlePlayer> Players { get; } = [];

    public void Setup() {
        TypeHandler.Setup();

        BattleModeTemplate battleModeTemplate = Properties.BattleMode switch {
            BattleMode.DM => new DMTemplate(),
            BattleMode.TDM => throw new NotImplementedException(),
            BattleMode.CTF => throw new NotImplementedException(),
            _ => throw new UnreachableException()
        };

        BattleEntity = battleModeTemplate.Create(LobbyEntity, Properties.ScoreLimit, Properties.TimeLimit * 60, 60);
        RoundEntity = new RoundTemplate().Create(BattleEntity);

        LobbyChatEntity = new BattleLobbyChatTemplate().Create();
        BattleChatEntity = new GeneralBattleChatTemplate().Create();

        // todo height maps (or server physics)

        ModeHandler = Properties.BattleMode switch {
            BattleMode.DM => new DMHandler(this),
            BattleMode.TDM => throw new NotImplementedException(),
            BattleMode.CTF => throw new NotImplementedException(),
            _ => throw new UnreachableException()
        };
    }

    public void UpdateProperties(BattleProperties properties) {
        ModeHandler previousHandler = ModeHandler;

        Properties = properties;
        MapInfo = ConfigManager.MapInfos.Values.Single(map => map.MapId == Properties.MapId);
        MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == Properties.MapId);

        LobbyEntity.RemoveComponent<MapGroupComponent>();
        LobbyEntity.RemoveComponent<BattleModeComponent>();
        LobbyEntity.RemoveComponent<UserLimitComponent>();
        LobbyEntity.RemoveComponent<GravityComponent>();

        LobbyEntity.AddComponent(new MapGroupComponent(MapEntity));
        LobbyEntity.AddComponent(new BattleModeComponent(Properties.BattleMode));
        LobbyEntity.AddComponent(new UserLimitComponent(Properties.MaxPlayers));
        LobbyEntity.AddComponent(new GravityComponent(Properties.Gravity));

        if (IsCustom) {
            LobbyEntity.RemoveComponent<ClientBattleParamsComponent>();
            LobbyEntity.AddComponent(new ClientBattleParamsComponent(Properties));
        }

        Setup();
        ModeHandler.TransferParameters(previousHandler);
    }

    public void Start() {
        // todo modules

        // todo teams

        foreach (BattlePlayer battlePlayer in Players.Where(player => !player.IsSpectator))
            battlePlayer.Init();
    }

    public void Finish() {
        StateManager.SetState(new Ended(StateManager));

        ModeHandler.OnFinished();

        // todo sum up results
    }

    public void Tick(double deltaTime) {
        Timer -= deltaTime;

        ModeHandler.Tick();
        TypeHandler.Tick();
        StateManager.Tick();

        foreach (BattlePlayer battlePlayer in Players.ToArray())
            battlePlayer.Tick();
    }

    public void AddPlayer(IPlayerConnection connection, bool spectator = false) { // todo squads
        if (connection.InBattle || !spectator && !CanAddPlayers) return;

        connection.Logger.Warning("Joining battle {Id}", Id);

        if (spectator) {
            connection.BattlePlayer = new BattlePlayer(connection, this, null, true);
        } else {
            Preset preset = connection.Player.CurrentPreset;

            connection.Share(LobbyEntity, LobbyChatEntity);
            connection.User.AddComponent(new BattleLobbyGroupComponent(LobbyEntity));
            connection.User.AddComponent(new UserEquipmentComponent(preset.Weapon.Id, preset.Hull.Id));

            foreach (BattlePlayer battlePlayer in Players) {
                battlePlayer.PlayerConnection.Share(connection.User);
                connection.Share(battlePlayer.PlayerConnection.User);
            }

            connection.BattlePlayer = ModeHandler.SetupBattlePlayer(connection);
            TypeHandler.PlayerEntered(connection.BattlePlayer);
        }

        Players.Add(connection.BattlePlayer);
        WasPlayers = true;

        if (spectator)
            connection.BattlePlayer.Init();
    }

    public void RemovePlayer(BattlePlayer battlePlayer) { // todo
        IPlayerConnection connection = battlePlayer.PlayerConnection;
        IEntity user = connection.User;

        connection.Unshare(BattleEntity, RoundEntity, BattleChatEntity);

        foreach (IEntity entity in Players
                     .Where(player => player.InBattleAsTank &&
                                      player != battlePlayer)
                     .SelectMany(player => player.Tank!.Entities))
            connection.Unshare(entity);

        user.RemoveComponent<BattleGroupComponent>();
        ModeHandler.PlayerExited(battlePlayer);

        if (battlePlayer.IsSpectator) {
            connection.Unshare(battlePlayer.BattleUser);
            RemovePlayerFromLobby(battlePlayer);
        } else {
            foreach (BattlePlayer player in Players.Where(player => player.InBattle))
                player.PlayerConnection.Unshare(battlePlayer.Tank!.Entities);

            Players.Remove(battlePlayer);

            if (user.HasComponent<MatchMakingUserReadyComponent>())
                user.RemoveComponent<MatchMakingUserReadyComponent>();

            if (!IsCustom && Players.All(player => player.IsSpectator)) {
                foreach (BattlePlayer spectator in Players) {
                    spectator.PlayerConnection.Send(new KickFromBattleEvent(), spectator.BattleUser);
                    RemovePlayer(spectator);
                }
            }

            if (!IsCustom)
                RemovePlayerFromLobby(battlePlayer);
        }
    }

    public void RemovePlayerFromLobby(BattlePlayer battlePlayer) {
        Players.Remove(battlePlayer);

        IPlayerConnection connection = battlePlayer.PlayerConnection;

        if (battlePlayer.IsSpectator) {
            foreach (IPlayerConnection otherConnection in Players.Select(player => player.PlayerConnection))
                connection.Unshare(otherConnection.User);
        } else {
            IEntity user = connection.User;

            TypeHandler.PlayerExited(battlePlayer);
            ModeHandler.RemoveBattlePlayer(battlePlayer);

            user.RemoveComponent<UserEquipmentComponent>();
            user.RemoveComponent<BattleLobbyGroupComponent>();
            connection.Unshare(LobbyEntity, LobbyChatEntity);

            if (user.HasComponent<MatchMakingUserReadyComponent>())
                user.RemoveComponent<MatchMakingUserReadyComponent>();

            foreach (IPlayerConnection otherConnection in Players.Select(player => player.PlayerConnection)) {
                otherConnection.Unshare(user);
                connection.Unshare(otherConnection.User);
            }

            connection.Logger.Warning("Left battle {Id}", Id);
        }

        connection.BattlePlayer = null;
    }
}