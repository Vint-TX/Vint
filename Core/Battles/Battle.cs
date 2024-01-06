using System.Diagnostics;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Type;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Time;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.ECS.Templates.Battle.Mode;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Server;

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

    public long Id => BattleEntity.Id;
    public bool IsCustom { get; }
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

        foreach (BattlePlayer battlePlayer in Players)
            battlePlayer.Tick();
    }

    public void AddPlayer(IPlayerConnection player, bool spectator = false) { // todo spectator
        if (player.InBattle) return;

        player.Logger.Warning("Joining battle {Id}", Id);

        if (spectator) {
            player.BattlePlayer = new BattlePlayer(player, this, null, true);
        } else {
            Preset preset = player.Player.CurrentPreset;

            player.Share(LobbyEntity, LobbyChatEntity);
            player.User.AddComponent(new BattleLobbyGroupComponent(LobbyEntity));
            player.User.AddComponent(new UserEquipmentComponent(preset.Weapon.Id, preset.Hull.Id));

            foreach (BattlePlayer battlePlayer in Players)
                battlePlayer.PlayerConnection.Share(player.User);

            player.BattlePlayer = ModeHandler.SetupBattlePlayer(player);
            TypeHandler.PlayerEntered(player.BattlePlayer);
        }

        Players.Add(player.BattlePlayer);

        if (spectator)
            player.BattlePlayer.Init();
    }
}