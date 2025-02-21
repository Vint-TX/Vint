using System.Numerics;
using ConcurrentCollections;
using LinqToDB;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Mode.Solo;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Modules;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank.Temperature;
using Vint.Core.Battle.Weapons;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Incarnation;
using Vint.Core.ECS.Components.Battle.Movement;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.ECS.Events.Battle.Damage;
using Vint.Core.ECS.Events.Battle.Effect.EMP;
using Vint.Core.ECS.Events.Battle.Module;
using Vint.Core.ECS.Events.Battle.Movement;
using Vint.Core.ECS.Events.Battle.Score;
using Vint.Core.ECS.Events.Battle.Score.Visual;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.ECS.Templates.Battle.Weapon;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.Server.Game;
using Vint.Core.Utils;
using HealthComponent = Vint.Core.ECS.Components.Battle.Parameters.Health.HealthComponent;
using SpeedComponent = Vint.Core.ECS.Components.Battle.Parameters.Chassis.SpeedComponent;

namespace Vint.Core.Battle.Tank;

public class BattleTank : IDisposable {
    public BattleTank(Tanker tanker) {
        Tanker = tanker;

        Entities = new TankEntities(this);
        TemperatureProcessor = new TemperatureProcessor(this);
        StateManager = new TankStateManager(this);
        Statistics = new BattleTankStatistics();

        WeaponHandler = Entities.Weapon.TemplateAccessor!.Template switch {
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
            _ => throw new ArgumentOutOfRangeException()
        };

        Modules = [];

        SpeedComponent = Entities.Tank.GetComponent<SpeedComponent>().Clone();
        Health = TotalHealth = MaxHealth = Entities.Tank.GetComponent<HealthComponent>().MaxHealth;
        BattleEnterTime = DateTimeOffset.UtcNow;
    }

    public static IReadOnlyDictionary<int, int> KillStreakToScore { get; } = new Dictionary<int, int> {
        { 2, 0 }, { 3, 5 }, { 4, 7 }, { 5, 10 }, { 10, 10 }, { 15, 10 }, { 20, 20 }, { 25, 30 }, { 30, 40 }, { 35, 50 }, { 40, 60 }, { 45, 70 }
    };

    public Round Round => Tanker.Round;
    public long CollisionsPhase { get; set; } = -1;

    public float SupplyDurationMultiplier { get; set; } = 1;
    public float ModuleCooldownCoeff { get; private set; } = 1;

    public ConcurrentHashSet<Effect> Effects { get; } = [];

    public DateTimeOffset BattleEnterTime { get; }
    public BattleTankStatistics Statistics { get; }
    public UserResult Result { get; private set; } = null!;
    public Dictionary<BattleTank, float> KillAssistants { get; } = new();

    public float Health { get; private set; }
    public float TotalHealth { get; set; }
    public float MaxHealth { get; }

    public TemperatureProcessor TemperatureProcessor { get; }
    public SpeedComponent SpeedComponent { get; }

    public Vector3 PreviousPosition { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; }

    public DateTimeOffset? SelfDestructTime { get; set; }

    public SpawnPoint PreviousSpawnPoint { get; private set; }
    public SpawnPoint SpawnPoint { get; private set; }

    public List<BattleModule> Modules { get; }
    public TankWeaponHandler WeaponHandler { get; }
    public Tanker Tanker { get; }
    public TankStateManager StateManager { get; set; }
    public TankEntities Entities { get; }

    public async Task Tick(TimeSpan deltaTime) {
        if (Tanker.IsPaused && DateTimeOffset.UtcNow > Tanker.KickTime) {
            await Tanker.Send(new KickFromBattleEvent(), Tanker.BattleUser);
            await Round.RemoveTanker(Tanker);
            return;
        }

        if (SelfDestructTime.HasValue && SelfDestructTime.Value <= DateTimeOffset.UtcNow)
            await SelfDestruct();

        await StateManager.Tick(deltaTime);
        await WeaponHandler.Tick(deltaTime);
        await TemperatureProcessor.Tick(deltaTime);

        foreach (Effect effect in Effects)
            await effect.Tick(deltaTime);

        foreach (BattleModule module in Modules)
            await module.Tick(deltaTime);
    }

    public async Task Enable() {
        await WeaponHandler.OnTankEnable();
        await Entities.Tank.AddComponent<TankMovableComponent>();
    }

    public async Task Disable() {
        foreach (Effect effect in Effects.Where(effect => effect.CanBeDeactivated)) {
            effect.UnScheduleAll();
            await effect.Deactivate();
        }

        foreach (IModuleWithoutEffect moduleWithoutEffect in Modules
                     .OfType<IModuleWithoutEffect>()
                     .Where(module => module.CanBeDeactivated)) {
            await moduleWithoutEffect.Deactivate();
        }

        TotalHealth = MaxHealth;
        await TemperatureProcessor.ResetAll();
        await Tanker.Send(new ResetTankSpeedEvent(), Entities.Tank);

        if (Entities.Tank.HasComponent<SelfDestructionComponent>()) {
            await Entities.Tank.RemoveComponent<SelfDestructionComponent>();
            SelfDestructTime = null;
        }

        await Entities.Tank.RemoveComponentIfPresent<TankMovableComponent>();
        await WeaponHandler.OnTankDisable();
    }

    public async Task ResetAndDisable() {
        foreach (Effect effect in Effects) {
            effect.CanBeDeactivated = true;
            effect.UnScheduleAll();
            await effect.Deactivate();
        }

        foreach (IModuleWithoutEffect moduleWithoutEffect in Modules.OfType<IModuleWithoutEffect>()) {
            moduleWithoutEffect.CanBeDeactivated = true;
            await moduleWithoutEffect.Deactivate();
        }

        TotalHealth = MaxHealth;
        await TemperatureProcessor.ResetAll();
        await Tanker.Send(new ResetTankSpeedEvent(), Entities.Tank);

        if (Entities.Tank.HasComponent<SelfDestructionComponent>()) {
            await Entities.Tank.RemoveComponent<SelfDestructionComponent>();
            SelfDestructTime = null;
        }

        await StateManager.CurrentState.ForceRemoveStateComponent();

        foreach (BattleModule module in Modules)
            await module.SetAmmo(module.MaxAmmo);

        await Entities.Tank.RemoveComponentIfPresent<TankMovableComponent>();
        await WeaponHandler.OnTankDisable();
    }

    public void SetSpawnPoint(SpawnPoint spawnPoint) {
        PreviousSpawnPoint = SpawnPoint;
        SpawnPoint = spawnPoint;
    }

    public async Task UpdateModuleCooldownSpeed(float coeff) {
        ModuleCooldownCoeff = coeff;
        await Tanker.BattleUser.ChangeComponent<BattleUserInventoryCooldownSpeedComponent>(component => component.SpeedCoeff = coeff);
        await Tanker.Send(new BattleUserInventoryCooldownSpeedChangedEvent(), Tanker.BattleUser);
    }

    public async Task EMPLock(TimeSpan duration) {
        IPlayerConnection connection = Tanker.Connection;
        IEntity debuffEffect = new EMPDebuffEffectTemplate().Create(Tanker, duration);

        await connection.Share(debuffEffect);
        connection.Schedule(duration, async () => await connection.Unshare(debuffEffect));

        foreach (BattleModule module in Modules)
            await module.EMPLock(duration);

        foreach (Effect effect in Effects)
            await effect.DeactivateByEMP();

        await Round.Players.Send(new EMPEffectReadyEvent(), Entities.Tank);
    }

    public async Task SetHealth(float health) {
        float before = Health;
        Health = Math.Clamp(health, 0, MaxHealth);
        await Entities.Tank.ChangeComponent<HealthComponent>(component => component.CurrentHealth = MathF.Ceiling(Health));

        await Round.Players.Send(new HealthChangedEvent(), Entities.Tank);

        foreach (IHealthModule healthModule in Modules.OfType<IHealthModule>())
            await healthModule.OnHealthChanged(before, Health, MaxHealth);
    }

    public async Task UpdateSpeed() {
        float temperature = TemperatureProcessor.Temperature;

        IEntity weapon = Entities.Weapon;
        IEntity tank = Entities.Tank;

        if (temperature < 0) {
            float minSpeed = TankUtils.CalculateFrozenSpeed(SpeedComponent.Speed, 12.5f);
            float minTurnSpeed = TankUtils.CalculateFrozenSpeed(SpeedComponent.TurnSpeed, 2.5f);
            float minWeaponSpeed = TankUtils.CalculateFrozenSpeed(WeaponHandler.WeaponRotationComponent.Speed, 7.5f);

            float newSpeed = MathUtils.Map(temperature, 0, -1, SpeedComponent.Speed, minSpeed);
            float newTurnSpeed = MathUtils.Map(temperature, 0, -1, SpeedComponent.TurnSpeed, minTurnSpeed);
            float newWeaponSpeed = MathUtils.Map(temperature, 0, -1, WeaponHandler.WeaponRotationComponent.Speed, minWeaponSpeed);

            await tank.ChangeComponent<SpeedComponent>(component => {
                component.Speed = newSpeed;
                component.TurnSpeed = newTurnSpeed;
            });

            await weapon.ChangeComponent<WeaponRotationComponent>(component => component.Speed = newWeaponSpeed);
        } else {
            await tank.ChangeComponent(SpeedComponent.Clone());
            await weapon.ChangeComponent(WeaponHandler.WeaponRotationComponent.Clone());
            await Tanker.Send(new ResetTankSpeedEvent(), tank);
        }
    }

    public bool IsEnemy(BattleTank other) => other != null! &&
                                             this != other &&
                                             (Round.Properties.FriendlyFire ||
                                              Round.Properties.BattleMode == BattleMode.DM ||
                                              !IsSameTeam(other));

    public bool IsSameTeam(BattleTank other) => other != null! &&
                                                Tanker.TeamColor == other.Tanker.TeamColor;

    public async Task KillBy(BattleTank killer, IEntity weapon) {
        const int baseScore = 10;

        float coeff = TotalHealth / MaxHealth;

        Database.Models.Player currentPlayer = Tanker.Connection.Player;
        Dictionary<BattleTank, float> assistants = KillAssistants
            .Where(assist => assist.Key != this)
            .ToDictionary();

        await SelfKill();
        await Round.Players.Send(new KillEvent(weapon, Entities.Tank), killer.Tanker.BattleUser);

        await killer.TryTerminateKillStreak(this); // maybe change execution order
        await killer.AddKills(1);
        await AddDeaths(1);

        foreach ((BattleTank assistant, float damageDealt) in assistants) {
            float damage = Math.Min(damageDealt, TotalHealth);
            int score = Convert.ToInt32(Math.Round(MathUtils.Map(damage, 0, TotalHealth, 1, baseScore * coeff)));

            if (assistant == killer) {
                score += 5;

                await killer.AddScore(score);

                await killer.Tanker.Send(
                    new VisualScoreKillEvent(Tanker.GetScoreWithBonus(score), currentPlayer.Username, currentPlayer.Rank),
                    killer.Tanker.BattleUser);
            } else {
                int percent = Convert.ToInt32(Math.Round(damage / TotalHealth * 100));

                assistant.Statistics.KillAssists++;
                await assistant.AddScore(score);
                await assistant.CommitStatistics();

                await assistant.Tanker.Send(
                    new VisualScoreAssistEvent(Tanker.GetScoreWithBonus(score), percent, currentPlayer.Username),
                    assistant.Tanker.BattleUser);
            }
        }

        await killer.CommitStatistics();
        await CommitStatistics();

        foreach (IKillModule killModule in killer.Modules.OfType<IKillModule>())
            await killModule.OnKill(this);

        if (Round.ModeHandler is TDMHandler tdm)
            await tdm.UpdateScore(killer.Tanker.TeamColor, 1);

        if (Round.Properties.Type is not BattleType.Rating)
            return;

        Database.Models.Player player = killer.Tanker.Connection.Player;

        await using DbConnection db = new();
        await db.BeginTransactionAsync();

        await db.Hulls
            .Where(hull => hull.PlayerId == player.Id && hull.Id == player.CurrentPreset.Hull.Id)
            .Set(hull => hull.Kills, hull => hull.Kills + 1)
            .UpdateAsync();

        await db.Weapons
            .Where(w => w.PlayerId == player.Id && w.Id == player.CurrentPreset.Weapon.Id)
            .Set(w => w.Kills, w => w.Kills + 1)
            .UpdateAsync();

        await db.Statistics
            .Where(stats => stats.PlayerId == player.Id)
            .Set(stats => stats.Kills, stats => stats.Kills + 1)
            .UpdateAsync();

        await db.SeasonStatistics
            .Where(stats => stats.PlayerId == player.Id)
            .Where(stats => stats.SeasonNumber == ConfigManager.ServerConfig.SeasonNumber)
            .Set(stats => stats.Kills, stats => stats.Kills + 1)
            .UpdateAsync();

        await db.CommitTransactionAsync();
    }

    public async Task SelfDestruct() {
        await SelfKill();
        SelfDestructTime = null;

        await Round.Players.Send(new SelfDestructionBattleUserEvent(), Tanker.BattleUser);

        await AddKills(-1);
        await AddScore(-10);
        await AddDeaths(1);
        await CommitStatistics();

        if (Round.ModeHandler is TDMHandler tdm)
            await tdm.UpdateScore(Tanker.TeamColor, -1);
    }

    async Task SelfKill() {
        foreach (IDeathModule deathModule in Modules.OfType<IDeathModule>())
            await deathModule.OnDeath();

        await Tanker.Send(new SelfTankExplosionEvent(), Entities.Tank);
        await StateManager.SetState(new Dead(StateManager));
        KillAssistants.Clear();

        if (Round.Properties.Type != BattleType.Rating) return;

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == Tanker.Connection.Player.Id)
            .Set(stats => stats.Deaths, stats => stats.Deaths + 1)
            .UpdateAsync();
    }

    public async Task AddKills(int delta) {
        await Entities.RoundUser.ChangeComponent<RoundUserStatisticsComponent>(component => component.Kills = Math.Max(0, component.Kills + delta));

        if (delta > 0)
            await Entities.Incarnation.ChangeComponent<TankIncarnationKillStatisticsComponent>(component => component.Kills += delta);

        await UpdateKillStreak();
    }

    public async Task AddDeaths(int delta) =>
        await Entities.RoundUser.ChangeComponent<RoundUserStatisticsComponent>(component => component.Deaths = Math.Max(0, component.Deaths + delta));

    public async Task AddScore(int deltaWithoutBonus) {
        await Entities.RoundUser.ChangeComponent<RoundUserStatisticsComponent>(component =>
            component.ScoreWithoutBonuses = Math.Max(0, component.ScoreWithoutBonuses + deltaWithoutBonus));

        if (Round.ModeHandler is SoloHandler soloHandler)
            await soloHandler.TryUpdateScore();

        if (deltaWithoutBonus <= 0 ||
            Round.Properties.Type != BattleType.Rating ||
            Round.StateManager.CurrentState is not Running)
            return;

        int deltaWithBonus = Tanker.GetScoreWithBonus(deltaWithoutBonus);
        IPlayerConnection connection = Tanker.Connection;

        await connection.ChangeExperience(deltaWithBonus);

        if (Round.BonusProcessor != null)
            await Round.BonusProcessor.GoldProcessor.ScoreChanged(deltaWithBonus);
    }

    public async Task CommitStatistics() {
        await Round.Players.Send(new RoundUserStatisticsUpdatedEvent(), Entities.RoundUser);
        await Round.ModeHandler.SortAllPlayers();
    }

    async Task UpdateKillStreak() {
        int killStreak = Entities.Incarnation.GetComponent<TankIncarnationKillStatisticsComponent>().Kills;
        Statistics.MaxKillStrike = Math.Max(Statistics.MaxKillStrike, killStreak);

        if (killStreak < 2) return;

        int score = KillStreakToScore.GetValueOrDefault(killStreak, KillStreakToScore.Values.Last());

        await AddScore(score);
        await CommitStatistics();
        await Tanker.Send(new VisualScoreStreakEvent(Tanker.GetScoreWithBonus(score)), Entities.BattleUser);

        if (killStreak < 5 || killStreak % 5 == 0)
            await Round.Players.Send(new KillStreakEvent(score), Entities.Incarnation);
    }

    async Task TryTerminateKillStreak(BattleTank target) {
        int targetKillStreak = target.Entities.Incarnation.GetComponent<TankIncarnationKillStatisticsComponent>().Kills;

        if (targetKillStreak < 2) return;

        await Tanker.Send(new StreakTerminationEvent(target.Tanker.Connection.Player.Username), Tanker.BattleUser);
    }

    public async Task ResetStatistics() {
        Statistics.Reset();
        await Entities.RoundUser.ChangeComponent(new RoundUserStatisticsComponent());
        await Round.Players.Send(new RoundUserStatisticsUpdatedEvent(), Entities.RoundUser);
    }

    public void CreateUserResult() =>
        Result = new UserResult(Tanker);

    public async Task Init() {
        await StateManager.Init();

        if (!Round.Properties.DisabledModules)
            await InitModules();
    }

    public async Task DeInit() {
        if (!Round.Properties.DisabledModules) {
            foreach (BattleModule module in Modules)
                await module.SwitchToUserEntities();
        }
    }

    async Task InitModules() {
        IPlayerConnection connection = Tanker.Connection;
        Preset preset = connection.Player.CurrentPreset;

        foreach (PresetModule presetModule in preset.Modules) {
            BattleModule module = ModuleRegistry.Get(presetModule.Entity.Id);
            await module.Init(this, presetModule.GetSlotEntity(connection), presetModule.Entity);
            Modules.Add(module);
        }

        IEntity goldModuleEntity = GlobalEntities.GetEntity("modules", "Gold");
        IEntity goldSlotEntity = connection.SharedEntities.Single(entity => entity.TemplateAccessor?.Template is SlotUserItemTemplate &&
                                                                            entity.GetComponent<SlotUserItemInfoComponent>().Slot == Slot.Slot7);

        BattleModule gold = ModuleRegistry.Get(goldModuleEntity.Id);
        await gold.Init(this, goldSlotEntity, goldModuleEntity);
        Modules.Add(gold);

        foreach (BattleModule module in Modules)
            await module.SwitchToBattleEntities();
    }

    public override int GetHashCode() => Tanker.GetHashCode();

    void Dispose(bool disposing) {
        if (disposing) { // todo dispose entities
            Modules.Clear();
            Effects.Clear();
            KillAssistants.Clear();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BattleTank() => Dispose(false);
}
