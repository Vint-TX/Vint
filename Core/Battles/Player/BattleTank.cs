using System.Diagnostics;
using System.Numerics;
using ConcurrentCollections;
using LinqToDB;
using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Results;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Type;
using Vint.Core.Battles.Weapons;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Incarnation;
using Vint.Core.ECS.Components.Battle.Movement;
using Vint.Core.ECS.Components.Battle.Parameters.Chassis;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.ECS.Events.Battle.Damage;
using Vint.Core.ECS.Events.Battle.Score;
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
using HealthComponent = Vint.Core.ECS.Components.Battle.Parameters.Health.HealthComponent;

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

        OriginalTemperatureConfigComponent = ConfigManager.GetComponent<TemperatureConfigComponent>(Tank.TemplateAccessor!.ConfigPath!);
        TemperatureConfig = (TemperatureConfigComponent)((IComponent)OriginalTemperatureConfigComponent).Clone();

        Tank.ChangeComponent<HealthComponent>(component => {
            component.CurrentHealth = Health;
            component.MaxHealth = MaxHealth;
        });

        UserResult = new UserResult(BattlePlayer);
        BattleEnterTime = DateTimeOffset.UtcNow;
    }

    public static IReadOnlyDictionary<int, int> KillStreakToScore { get; } = new Dictionary<int, int> {
        { 2, 0 }, { 3, 5 }, { 4, 7 }, { 5, 10 }, { 10, 10 }, { 15, 10 }, { 20, 20 }, { 25, 30 }, { 30, 40 }, { 35, 50 }, { 40, 60 }
    };

    public long CollisionsPhase { get; set; } = -1;

    public ConcurrentHashSet<Effect> Effects { get; } = [];

    public DateTimeOffset BattleEnterTime { get; }
    public UserResult UserResult { get; }
    public Dictionary<BattleTank, float> KillAssistants { get; } = new();
    public ConcurrentHashSet<TemperatureAssist> TemperatureAssists { get; } = [];
    public float DealtDamage { get; set; }
    public float TakenDamage { get; set; }

    public float Health { get; private set; }
    public float MaxHealth { get; }

    public TemperatureConfigComponent TemperatureConfig { get; private set; }
    public float Temperature { get; private set; }

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

    public SpeedComponent OriginalSpeedComponent { get; }
    TemperatureConfigComponent OriginalTemperatureConfigComponent { get; }

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
        }

        foreach (Effect effect in Effects)
            effect.Tick();

        WeaponHandler.Tick();
        HandleTemperature();
    }

    public void Enable() { // todo modules
        if (FullDisabled) return;

        WeaponHandler.OnTankEnable();
        Tank.AddComponent(new TankMovableComponent());
        
        TemperatureConfig = (TemperatureConfigComponent)((IComponent)OriginalTemperatureConfigComponent).Clone();
        TemperatureAssists.Clear();
        
        SetTemperature(0);
        Tank.ChangeComponent(((IComponent)OriginalSpeedComponent).Clone());
    }

    public void Disable(bool full) { // todo modules
        FullDisabled = full;
        TemperatureConfig = (TemperatureConfigComponent)((IComponent)OriginalTemperatureConfigComponent).Clone();

        foreach (Effect effect in Effects) {
            effect.UnScheduleAll();
            effect.Deactivate();
        }
        
        TemperatureAssists.Clear();
        SetTemperature(0);
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
        Tank.ChangeComponent<HealthComponent>(component => component.CurrentHealth = MathF.Ceiling(Health));

        /*HealthComponent healthComponent = Tank.GetComponent<HealthComponent>();
        healthComponent.CurrentHealth = MathF.Ceiling(Health);

        Tank.RemoveComponent<HealthComponent>();
        Tank.AddComponent(healthComponent);*/

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
            battlePlayer.PlayerConnection.Send(new HealthChangedEvent(), Tank);
    }

    public void SetTemperature(float temperature) { // todo modules
        Temperature = Math.Clamp(temperature, TemperatureConfig.MinTemperature, TemperatureConfig.MaxTemperature);
        Tank.ChangeComponent<TemperatureComponent>(component => component.Temperature = Temperature);

        UpdateSpeed();
    }

    public void HandleTemperature() {
        if (Battle.StateManager.CurrentState is Ended) return;

        if (StateManager.CurrentState is Dead) {
            TemperatureAssists.Clear();
            if (Temperature != 0) SetTemperature(0);
            return;
        }

        TimeSpan period = TimeSpan.FromMilliseconds(TemperatureConfig.TactPeriodInMs);

        if (TemperatureAssists.Count > 0) {
            float newTemperature = TemperatureAssists.Sum(ass => ass.CurrentTemperature);
            SetTemperature(newTemperature);
        }

        foreach (TemperatureAssist assist in TemperatureAssists) {
            if (StateManager.CurrentState is Dead) {
                TemperatureAssists.Clear();
                SetTemperature(0);
                break;
            }

            if (DateTimeOffset.UtcNow - assist.LastTick < period) continue;

            float temperatureDelta = assist.CurrentTemperature switch {
                > 0 => -TemperatureConfig.AutoDecrementInMs,
                < 0 => TemperatureConfig.AutoIncrementInMs,
                _ => 0
            } * TemperatureConfig.TactPeriodInMs;

            if (assist is { CurrentTemperature: > 0, Weapon: not IsisWeaponHandler } && (assist.Assistant == this || IsEnemy(assist.Assistant))) {
                float value = MathF.Round(MathUtils.Map(assist.CurrentTemperature, 0, assist.Weapon.TemperatureLimit, 0, assist.MaxDamage));

                CalculatedDamage damage = new(default, value, false, false, false, false);
                Battle.DamageProcessor.Damage(assist.Assistant, this, ((WeaponHandler)assist.Weapon).MarketEntity, damage);
            }

            bool wasPositive = assist.CurrentTemperature > 0;
            assist.CurrentTemperature += temperatureDelta;
            bool isPositive = assist.CurrentTemperature > 0;

            if (wasPositive != isPositive) {
                TemperatureAssists.TryRemove(assist);

                if (TemperatureAssists.Count == 0)
                    SetTemperature(0);
            }

            assist.LastTick = DateTimeOffset.UtcNow;
        }
    }

    public void UpdateTemperatureAssists(BattleTank assistant, bool normalizeOnly) { // todo modules
        if (assistant.WeaponHandler is not ITemperatureWeaponHandler temperatureWeapon) return;

        float maxHeatDamage = (temperatureWeapon as IHeatWeaponHandler)?.HeatDamage ?? 0;
        float temperatureDelta = temperatureWeapon switch {
            IsisWeaponHandler isis => Temperature switch {
                < 0 => isis.IncreaseFriendTemperature,
                > 0 => -isis.DecreaseFriendTemperature,
                _ => 0
            },
            _ => temperatureWeapon.TemperatureDelta
        };

        temperatureDelta =
            Math.Clamp(Temperature + temperatureDelta, TemperatureConfig.MinTemperature, TemperatureConfig.MaxTemperature) - Temperature;

        if (temperatureDelta == 0) return;

        bool deltaIsPositive = temperatureDelta >= 0;

        if (Temperature - temperatureDelta >= 0 != deltaIsPositive) {
            foreach (TemperatureAssist assist in TemperatureAssists) {
                bool assistTemperatureIsPositive = assist.CurrentTemperature > 0;

                if (assistTemperatureIsPositive == deltaIsPositive) continue;

                if (assist.CurrentTemperature + temperatureDelta >= 0 != assistTemperatureIsPositive) {
                    TemperatureAssists.TryRemove(assist);

                    if (deltaIsPositive) temperatureDelta -= assist.CurrentTemperature;
                    else temperatureDelta += assist.CurrentTemperature;

                    deltaIsPositive = temperatureDelta > 0;
                    continue;
                }

                assist.CurrentTemperature += temperatureDelta;
            }

            if (TemperatureAssists.Count == 0)
                SetTemperature(0);
        }

        if (temperatureDelta == 0 || normalizeOnly) return;

        TemperatureAssist? sourceAssist = TemperatureAssists
            .SingleOrDefault(assist => assist.Assistant == assistant &&
                                       assist.Weapon == temperatureWeapon);

        if (sourceAssist == null) {
            sourceAssist = new TemperatureAssist(assistant, temperatureWeapon, maxHeatDamage, temperatureDelta, DateTimeOffset.UtcNow);
            TemperatureAssists.Add(sourceAssist);
        } else {
            float limit = sourceAssist.Weapon.TemperatureLimit;
            float newTemperature = sourceAssist.CurrentTemperature + temperatureDelta;

            sourceAssist.CurrentTemperature = limit switch {
                < 0 => Math.Clamp(newTemperature, limit, 0),
                > 0 => Math.Clamp(newTemperature, 0, limit),
                _ => 0
            };
        }
    }

    public void UpdateSpeed() {
        List<ISpeedEffect> speedEffects = Effects.OfType<ISpeedEffect>().ToList();

        if (speedEffects.Count > 0) {
            foreach (ISpeedEffect speedEffect in speedEffects)
                speedEffect.UpdateTankSpeed();
            
            return;
        }

        if (Temperature < 0) {
            float minTemperature = TemperatureConfig.MinTemperature;

            float newSpeed = MathUtils.Map(Temperature, 0, minTemperature, OriginalSpeedComponent.Speed, 0);
            float newTurnSpeed = MathUtils.Map(Temperature, 0, minTemperature, OriginalSpeedComponent.TurnSpeed, 0);
            float newWeaponSpeed = MathUtils.Map(Temperature, 0, minTemperature, WeaponHandler.OriginalWeaponRotationComponent.Speed, 0);

            Tank.ChangeComponent<SpeedComponent>(component => {
                component.Speed = newSpeed;
                component.TurnSpeed = newTurnSpeed;
            });
            Weapon.ChangeComponent<WeaponRotationComponent>(component => component.Speed = newWeaponSpeed);
        } else {
            Tank.ChangeComponent(((IComponent)OriginalSpeedComponent).Clone());
            Weapon.ChangeComponent(((IComponent)WeaponHandler.OriginalWeaponRotationComponent).Clone());
        }
    }

    public bool IsEnemy(BattleTank other) => this != other &&
                                             other != null! &&
                                             (Battle.Properties.BattleMode == BattleMode.DM ||
                                              BattlePlayer.TeamColor != other.BattlePlayer.TeamColor);

    public void KillBy(BattleTank killer, IEntity weapon) {
        Dictionary<BattleTank, float> assistants = KillAssistants.Where(assist => assist.Key != this && assist.Key != killer).ToDictionary();
        SelfKill();

        Database.Models.Player currentPlayer = BattlePlayer.PlayerConnection.Player;
        KillEvent killEvent = new(weapon, Tank);

        foreach (IPlayerConnection connection in Battle.Players
                     .Where(battlePlayer => battlePlayer.InBattle)
                     .Select(battlePlayer => battlePlayer.PlayerConnection)) {
            connection.Send(killEvent, killer.BattleUser);
        }

        killer.BattlePlayer.PlayerConnection.Send(
            new VisualScoreKillEvent(BattlePlayer.GetScoreWithBonus(10), currentPlayer.Username, currentPlayer.Rank),
            killer.BattleUser);

        killer.UpdateStatistics(1, 0, 0, 10);
        UpdateStatistics(0, 0, 1, 0);

        killer.UpdateKillStreak();
        ResetKillStreak(killer);

        foreach ((BattleTank assistant, float damageDealt) in assistants) {
            int percent = Convert.ToInt32(Math.Round(damageDealt / MaxHealth * 100));

            if (percent < 1) continue;

            int score = MathUtils.Map(percent, 1, 100, 1, 10);
            assistant.UpdateStatistics(0, 1, 0, score);
            assistant.BattlePlayer.PlayerConnection.Send(new VisualScoreAssistEvent(BattlePlayer.GetScoreWithBonus(score),
                percent,
                currentPlayer.Username));
        }

        switch (Battle.ModeHandler) {
            case TDMHandler tdm:
                tdm.UpdateScore(killer.BattlePlayer.Team, 1);
                break;

            case SoloHandler dm:
                dm.UpdateScore(null, 0);
                break;
        }

        if (Battle.TypeHandler is not MatchmakingHandler) return;

        Database.Models.Player player = killer.BattlePlayer.PlayerConnection.Player;

        using DbConnection db = new();
        db.BeginTransaction();

        db.Hulls
            .Where(hull => hull.PlayerId == player.Id &&
                           hull.Id == player.CurrentPreset.Hull.Id)
            .Set(hull => hull.Kills, hull => hull.Kills + 1)
            .Update();

        db.Weapons
            .Where(w => w.PlayerId == player.Id &&
                        w.Id == player.CurrentPreset.Weapon.Id)
            .Set(w => w.Kills, w => w.Kills + 1)
            .Update();

        db.Statistics
            .Where(stats => stats.PlayerId == player.Id)
            .Set(stats => stats.Kills, stats => stats.Kills + 1)
            .Update();

        db.SeasonStatistics
            .Where(stats => stats.PlayerId == player.Id)
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

        UpdateStatistics(-1, 0, 1, -10);
        ResetKillStreak();

        if (Battle.ModeHandler is TDMHandler)
            Battle.ModeHandler.UpdateScore(BattlePlayer.Team, -1);
    }

    void SelfKill() {
        StateManager.SetState(new Dead(StateManager));
        KillAssistants.Clear();

        if (Battle.TypeHandler is not MatchmakingHandler) return;

        using DbConnection db = new();
        db.Statistics
            .Where(stats => stats.PlayerId == BattlePlayer.PlayerConnection.Player.Id)
            .Set(stats => stats.Deaths, stats => stats.Deaths + 1)
            .Update();
    }

    public void UpdateStatistics(int kills, int assists, int deaths, int score) {
        RoundUser.ChangeComponent<RoundUserStatisticsComponent>(component => {
            component.Kills = Math.Max(0, component.Kills + kills);
            component.KillAssists = Math.Max(0, component.KillAssists + assists);
            component.Deaths = Math.Max(0, component.Deaths + deaths);
            component.ScoreWithoutBonuses = Math.Max(0, component.ScoreWithoutBonuses + score);
        });

        if (Battle.TypeHandler is MatchmakingHandler && score > 0) {
            BattlePlayer.PlayerConnection.ChangeExperience(BattlePlayer.GetScoreWithBonus(score));
            BattlePlayer.PlayerConnection.CheckRank();
        }

        if (kills > 0)
            Incarnation.ChangeComponent<TankIncarnationKillStatisticsComponent>(component => component.Kills += kills);

        foreach (IPlayerConnection connection in Battle.Players.Where(player => player.InBattle).Select(player => player.PlayerConnection))
            connection.Send(new RoundUserStatisticsUpdatedEvent(), RoundUser);

        Battle.ModeHandler.SortPlayers();
    }

    public void UpdateKillStreak() {
        int killStreak = Incarnation.GetComponent<TankIncarnationKillStatisticsComponent>().Kills;

        UserResult.KillStrike = Math.Max(UserResult.KillStrike, killStreak);
        if (killStreak < 2) return;

        int score = KillStreakToScore.GetValueOrDefault(killStreak, killStreak);

        RoundUser.ChangeComponent<RoundUserStatisticsComponent>(component => component.ScoreWithoutBonuses += score);
        BattlePlayer.PlayerConnection.Send(new VisualScoreStreakEvent(BattlePlayer.GetScoreWithBonus(score)), BattleUser);

        if (killStreak < 5 || killStreak % 5 == 0)
            BattlePlayer.PlayerConnection.Send(new KillStreakEvent(score), Incarnation);
    }

    public void ResetKillStreak(BattleTank? killer = null) {
        TankIncarnationKillStatisticsComponent incarnationStatisticsComponent =
            Incarnation.GetComponent<TankIncarnationKillStatisticsComponent>();

        if (incarnationStatisticsComponent.Kills >= 2 && killer != null)
            killer.BattlePlayer.PlayerConnection.Send(
                new StreakTerminationEvent(BattlePlayer.PlayerConnection.Player.Username),
                killer.BattleUser);

        incarnationStatisticsComponent.Kills = 0;
        Incarnation.ChangeComponent(incarnationStatisticsComponent);
    }

    public override int GetHashCode() => BattlePlayer.GetHashCode();
}