using System.Diagnostics;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
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

        Setup();
    }

    public Battle(BattleProperties properties, IPlayerConnection owner) { // Custom battle
        IsCustom = true;
        Properties = properties;

        TypeHandler = null!; // todo

        Setup();
    }

    public long Id => BattleEntity.Id;
    public bool IsCustom { get; }

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

    public void Tick(double deltaTime) {
        ModeHandler.Tick();
        TypeHandler.Tick();
    }

    public void AddPlayer(IPlayerConnection player, bool spectator = false) { // todo spectator
        if (player.IsInBattle) return;

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

            BattlePlayer tankPlayer = ModeHandler.SetupBattlePlayer(player);
            player.BattlePlayer = tankPlayer;

            TypeHandler.PlayerEntered(tankPlayer);
        }

        Players.Add(player.BattlePlayer);

        if (spectator)
            player.BattlePlayer.Init();
    }
}