using System.Diagnostics;
using System.Numerics;
using LinqToDB;
using Vint.Core.Battles.Results;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Weapons;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Movement;
using Vint.Core.ECS.Components.Battle.Parameters.Chassis;
using Vint.Core.ECS.Components.Battle.Parameters.Health;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.ECS.Events.Battle.Damage;
using Vint.Core.ECS.Events.Battle.Score.Visual;
using Vint.Core.ECS.Movement;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.ECS.Templates.Battle.Graffiti;
using Vint.Core.ECS.Templates.Battle.Incarnation;
using Vint.Core.ECS.Templates.Battle.Tank;
using Vint.Core.ECS.Templates.Battle.User;
using Vint.Core.ECS.Templates.Battle.Weapon;
using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Player;

public class BattleTank {
    public BattleTank(BattlePlayer battlePlayer) {
        BattlePlayer = battlePlayer;
        Battle = battlePlayer.Battle;
        StateManager = new TankStateManager(this);

        IPlayerConnection playerConnection = battlePlayer.PlayerConnection;
        Preset preset = playerConnection.Player.CurrentPreset;

        IEntity weapon = preset.Weapon;
        IEntity weaponSkin = preset.WeaponSkin;
        IEntity shell = preset.Shell;

        IEntity hull = preset.Hull;
        IEntity hullSkin = preset.HullSkin;

        IEntity cover = preset.Cover;
        IEntity paint = preset.Paint;
        IEntity graffiti = preset.Graffiti;

        OriginalSpeedComponent = ConfigManager.GetComponent<SpeedComponent>(hull.TemplateAccessor!.ConfigPath!);

        BattleUser = battlePlayer.BattleUser = new BattleUserTemplate().CreateAsTank(playerConnection.User, Battle.Entity, battlePlayer.Team);

        Tank = new TankTemplate().Create(hull, BattlePlayer.BattleUser);

        Weapon = weapon.TemplateAccessor!.Template switch {
            SmokyMarketItemTemplate => new SmokyBattleItemTemplate().Create(Tank, BattlePlayer),
            TwinsMarketItemTemplate => new TwinsBattleItemTemplate().Create(Tank, BattlePlayer),
            ThunderMarketItemTemplate => new ThunderBattleItemTemplate().Create(Tank, BattlePlayer),
            RailgunMarketItemTemplate => new RailgunBattleItemTemplate().Create(Tank, BattlePlayer),
            RicochetMarketItemTemplate => new RicochetBattleItemTemplate().Create(Tank, BattlePlayer),
            IsisMarketItemTemplate => new IsisBattleItemTemplate().Create(Tank, BattlePlayer),
            VulcanMarketItemTemplate => new VulcanBattleItemTemplate().Create(Tank, BattlePlayer),
            FreezeMarketItemTemplate => new FreezeBattleItemTemplate().Create(Tank, BattlePlayer),
            FlamethrowerMarketItemTemplate => new FlamethrowerBattleItemTemplate().Create(Tank, BattlePlayer),
            ShaftMarketItemTemplate => new ShaftBattleItemTemplate().Create(Tank, BattlePlayer),
            HammerMarketItemTemplate => new HammerBattleItemTemplate().Create(Tank, BattlePlayer),
            _ => throw new UnreachableException()
        };

        HullSkin = new HullSkinBattleItemTemplate().Create(hullSkin, Tank);
        WeaponSkin = new WeaponSkinBattleItemTemplate().Create(weaponSkin, Tank);
        Cover = new WeaponPaintBattleItemTemplate().Create(cover, Tank);
        Paint = new TankPaintBattleItemTemplate().Create(paint, Tank);
        Graffiti = new GraffitiBattleItemTemplate().Create(graffiti, Tank);
        Shell = new ShellBattleItemTemplate().Create(shell, Tank);

        // todo modules

        Incarnation = new TankIncarnationTemplate().Create(Tank);
        RoundUser = new RoundUserTemplate().Create(BattlePlayer, Tank);

        WeaponHandler = Weapon.TemplateAccessor!.Template switch {
            SmokyBattleItemTemplate => new SmokyWeaponHandler(this),
            TwinsBattleItemTemplate => new TwinsWeaponHandler(this),
            ThunderBattleItemTemplate => new ThunderWeaponHandler(this),
            RailgunBattleItemTemplate => new RailgunWeaponHandler(this),
            RicochetBattleItemTemplate => new RicochetWeaponHandler(this),
            IsisBattleItemTemplate => new IsisWeaponHandler(this),
            VulcanBattleItemTemplate => new VulcanWeaponHandler(this),
            FreezeBattleItemTemplate => new FreezeWeaponHandler(this),
            FlamethrowerBattleItemTemplate => new FlamethrowerWeaponHandler(this),
            ShaftBattleItemTemplate => new ShaftWeaponHandler(this),
            HammerBattleItemTemplate => new HammerWeaponHandler(this),
            _ => throw new UnreachableException()
        };

        MaxHealth = ConfigManager.GetComponent<HealthComponent>(hull.TemplateAccessor.ConfigPath!).MaxHealth;
        Health = MaxHealth;

        Tank.ChangeComponent<HealthComponent>(component => {
            component.CurrentHealth = Health;
            component.MaxHealth = MaxHealth;
        });

        UserResult = new UserResult(BattlePlayer);
        BattleEnterTime = DateTimeOffset.UtcNow;
    }

    public long CollisionsPhase { get; set; } = -1;

    public DateTimeOffset BattleEnterTime { get; }
    public UserResult UserResult { get; }
    public float DealtDamage { get; set; }
    public float TakenDamage { get; set; }

    public float Health { get; private set; }
    public float MaxHealth { get; }

    public Vector3 PreviousPosition { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; }

    public DateTimeOffset? SelfDestructTime { get; set; }
    public bool ForceSelfDestruct { get; set; }
    public bool FullDisabled { get; private set; }

    public SpawnPoint PreviousSpawnPoint { get; private set; } = null!;
    public SpawnPoint SpawnPoint { get; private set; } = null!;

    public WeaponHandler WeaponHandler { get; }
    public BattlePlayer BattlePlayer { get; }
    public Battle Battle { get; }
    public TankStateManager StateManager { get; set; }

    public IEnumerable<IEntity> Entities => [Incarnation, RoundUser, BattleUser, Tank, Weapon, HullSkin, WeaponSkin, Cover, Paint, Graffiti, Shell];

    public IEntity Incarnation { get; private set; }
    public IEntity RoundUser { get; }
    public IEntity BattleUser { get; }

    public IEntity Tank { get; }
    public IEntity Weapon { get; }

    public IEntity HullSkin { get; }
    public IEntity WeaponSkin { get; }

    public IEntity Cover { get; }
    public IEntity Paint { get; }

    public IEntity Graffiti { get; }
    public IEntity Shell { get; }

    SpeedComponent OriginalSpeedComponent { get; }

    public void Tick() {
        if (ForceSelfDestruct || SelfDestructTime.HasValue && SelfDestructTime.Value <= DateTimeOffset.UtcNow)
            SelfDestruct();

        StateManager.Tick();

        if (CollisionsPhase == Battle.Entity.GetComponent<BattleTankCollisionsComponent>().SemiActiveCollisionsPhase) {
            Tank.RemoveComponentIfPresent<TankStateTimeOutComponent>();
            Battle.Entity.ChangeComponent<BattleTankCollisionsComponent>(component =>
                component.SemiActiveCollisionsPhase++);

            StateManager.SetState(new Active(StateManager));
            SetHealth(MaxHealth);
        }

        if (BattlePlayer.IsPaused &&
            (!BattlePlayer.KickTime.HasValue ||
             DateTimeOffset.UtcNow > BattlePlayer.KickTime)) {
            BattlePlayer.IsPaused = false;
            BattlePlayer.KickTime = null;
            BattlePlayer.IsKicked = true;
            BattlePlayer.PlayerConnection.Send(new KickFromBattleEvent(), BattleUser);
            Battle.RemovePlayer(BattlePlayer);
            BattlePlayer.PlayerConnection.Logger.ForType(GetType()).Warning("Player kicked from battle (pause)");
        }

        WeaponHandler.Tick();
    }

    public void Enable() { // todo modules
        if (FullDisabled) return;

        WeaponHandler.OnTankEnable();
        Tank.AddComponent(new TankMovableComponent());
    }

    public void Disable(bool full) { // todo modules, temperature
        FullDisabled = full;
        Tank.ChangeComponent(((IComponent)OriginalSpeedComponent).Clone());

        if (Tank.HasComponent<SelfDestructionComponent>()) {
            Tank.RemoveComponent<SelfDestructionComponent>();
            SelfDestructTime = null;
            ForceSelfDestruct = false;
        }

        if (full)
            Tank.RemoveComponentIfPresent(StateManager.CurrentState.StateComponent);

        Tank.RemoveComponentIfPresent<TankMovableComponent>();
        WeaponHandler.OnTankDisable();
    }

    public void Spawn() {
        Tank.RemoveComponentIfPresent<TankVisibleStateComponent>();

        if (Tank.HasComponent<TankMovementComponent>()) {
            Tank.RemoveComponent<TankMovementComponent>();

            IEntity incarnation = Incarnation;
            Incarnation = new TankIncarnationTemplate().Create(Tank);

            foreach (IPlayerConnection playerConnection in incarnation.SharedPlayers) {
                playerConnection.Unshare(incarnation);
                playerConnection.Share(Incarnation);
            }
        }

        PreviousSpawnPoint = SpawnPoint;
        SpawnPoint = Battle.ModeHandler.GetRandomSpawnPoint(BattlePlayer);

        Movement movement = new() {
            Position = SpawnPoint.Position,
            Orientation = SpawnPoint.Rotation
        };

        Tank.AddComponent(new TankMovementComponent(movement, default, 0, 0));
    }

    public void SetHealth(float health) { // todo modules
        Health = Math.Clamp(health, 0, MaxHealth);

        HealthComponent healthComponent = Tank.GetComponent<HealthComponent>();
        healthComponent.CurrentHealth = (float)Math.Ceiling(Health);

        Tank.RemoveComponent<HealthComponent>();
        Tank.AddComponent(healthComponent);

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
            battlePlayer.PlayerConnection.Send(new HealthChangedEvent(), Tank);
    }

    public bool IsEnemy(BattleTank other) => this != other &&
                                             other != null! &&
                                             (Battle.Properties.BattleMode == BattleMode.DM ||
                                              BattlePlayer.TeamColor != other.BattlePlayer.TeamColor);

    public void KillBy(BattleTank killer, IEntity weapon) {
        SelfKill();

        Database.Models.Player currentPlayer = BattlePlayer.PlayerConnection.Player;
        KillEvent killEvent = new(weapon, Tank);

        foreach (IPlayerConnection connection in Battle.Players
                     .Where(battlePlayer => battlePlayer.InBattle)
                     .Select(battlePlayer => battlePlayer.PlayerConnection)
                     .ToList()) {
            connection.Send(killEvent, killer.BattleUser);
        }

        killer.BattlePlayer.PlayerConnection.Send(new VisualScoreKillEvent(0, currentPlayer.Username, currentPlayer.Rank), killer.BattleUser);

        if (Battle.IsCustom) return;

        using DbConnection db = new();
        db.BeginTransaction();

        db.Statistics
            .Where(stats => stats.PlayerId == killer.BattlePlayer.PlayerConnection.Player.Id)
            .Set(stats => stats.Kills, stats => stats.Kills + 1)
            .Update();

        db.SeasonStatistics
            .Where(stats => stats.PlayerId == killer.BattlePlayer.PlayerConnection.Player.Id)
            .Where(stats => stats.SeasonNumber == ConfigManager.SeasonNumber)
            .Set(stats => stats.Kills, stats => stats.Kills + 1)
            .Update();

        db.CommitTransaction();
    }

    public void SelfDestruct() {
        SelfKill();
        ForceSelfDestruct = false;
        SelfDestructTime = null;

        SelfDestructionBattleUserEvent selfDestructionEvent = new();

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
            battlePlayer.PlayerConnection.Send(selfDestructionEvent, BattleUser);
    }

    void SelfKill() {
        StateManager.SetState(new Dead(StateManager));

        if (Battle.IsCustom) return;

        using DbConnection db = new();
        db.Statistics
            .Where(stats => stats.PlayerId == BattlePlayer.PlayerConnection.Player.Id)
            .Set(stats => stats.Deaths, stats => stats.Deaths + 1)
            .Update();
    }

    public override int GetHashCode() => BattlePlayer.GetHashCode();
}