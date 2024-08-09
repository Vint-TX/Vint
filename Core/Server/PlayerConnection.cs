using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using ConcurrentCollections;
using LinqToDB;
using Serilog;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Components.Battle.Rewards;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.Preset;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.ECS.Events.Action;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.ECS.Events.Items;
using Vint.Core.ECS.Events.Items.Module;
using Vint.Core.ECS.Events.Ping;
using Vint.Core.ECS.Templates;
using Vint.Core.ECS.Templates.Avatar;
using Vint.Core.ECS.Templates.Containers;
using Vint.Core.ECS.Templates.Covers;
using Vint.Core.ECS.Templates.Entrance;
using Vint.Core.ECS.Templates.Gold;
using Vint.Core.ECS.Templates.Graffiti;
using Vint.Core.ECS.Templates.Hulls;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.ECS.Templates.Money;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.ECS.Templates.Paints;
using Vint.Core.ECS.Templates.Premium;
using Vint.Core.ECS.Templates.Preset;
using Vint.Core.ECS.Templates.Shells;
using Vint.Core.ECS.Templates.Skins;
using Vint.Core.ECS.Templates.User;
using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.ECS.Templates.Weapons.User;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Commands;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public interface IPlayerConnection {
    public ILogger Logger { get; }

    public GameServer Server { get; }
    public Player Player { get; set; }
    public BattlePlayer? BattlePlayer { get; set; }
    public IEntity User { get; }
    public IEntity ClientSession { get; }

    public bool IsOnline { get; }
    public bool InLobby { get; }
    public DateTimeOffset PingSendTime { set; }
    public DateTimeOffset PongReceiveTime { set; }
    public long Ping { get; }
    public Invite? Invite { get; set; }

    public string? RestorePasswordCode { get; set; }
    public bool RestorePasswordCodeValid { get; set; }

    public int BattleSeries { get; set; }

    public ConcurrentHashSet<IEntity> SharedEntities { get; }

    public Task Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration);

    public Task Login(
        bool saveAutoLoginToken,
        bool rememberMe,
        string hardwareFingerprint);

    public Task ChangePassword(string passwordDigest);

    public Task ChangeReputation(int delta);

    public Task CheckRankUp();

    public Task ChangeExperience(int delta);

    public Task ChangeGameplayChestScore(int delta);

    public Task PurchaseItem(IEntity marketItem, int amount, int price, bool forXCrystals, bool mount);

    public Task MountItem(IEntity userItem);

    public Task AssembleModule(IEntity marketItem);

    public Task UpgradeModule(IEntity userItem, bool forXCrystals);

    public Task<bool> OwnsItem(IEntity marketItem);

    public Task SetUsername(string username);

    public Task ChangeCrystals(long delta);

    public Task ChangeXCrystals(long delta);

    public Task SetGoldBoxes(int goldBoxes);

    public Task DisplayMessage(string message, TimeSpan? closeTime = null);

    public Task SetClipboard(string content);

    public ValueTask OpenURL(string url);

    public Task Kick(string? reason);

    public ValueTask Send(ICommand command);

    public ValueTask Send(IEvent @event);

    public ValueTask Send(IEvent @event, params IEntity[] entities);

    public Task Share(IEntity entity);

    public Task ShareIfUnshared(IEntity entity);

    public Task Unshare(IEntity entity);

    public Task UnshareIfShared(IEntity entity);

    public void Schedule(TimeSpan delay, Action action);

    public void Schedule(DateTimeOffset time, Action action);

    public void Schedule(TimeSpan delay, Func<Task> action);

    public void Schedule(DateTimeOffset time, Func<Task> action);

    public Task Tick();
}

public abstract class PlayerConnection(
    GameServer server
) : IPlayerConnection {
    public Guid Id { get; } = Guid.NewGuid();
    public ILogger Logger { get; protected set; } = Log.Logger.ForType(typeof(PlayerConnection));

    public GameServer Server { get; } = server;
    public Player Player { get; set; } = null!;
    public IEntity User { get; private set; } = null!;
    public IEntity ClientSession { get; protected set; } = null!;
    public BattlePlayer? BattlePlayer { get; set; }
    public int BattleSeries { get; set; }
    public ConcurrentHashSet<IEntity> SharedEntities { get; } = [];
    ConcurrentHashSet<DelayedAction> DelayedActions { get; } = [];
    ConcurrentHashSet<DelayedTask> DelayedTasks { get; } = [];

    public string? RestorePasswordCode { get; set; }
    public bool RestorePasswordCodeValid { get; set; }

    public abstract bool IsOnline { get; }
    public bool InLobby => BattlePlayer != null;
    public DateTimeOffset PingSendTime { get; set; }

    public DateTimeOffset PongReceiveTime {
        set => Ping = (long)(value - PingSendTime).TotalMilliseconds;
    }

    public long Ping { get; private set; }
    public Invite? Invite { get; set; }

    public async Task Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration) {
        Logger.Information("Registering player '{Username}'", username);

        byte[] passwordHash = new Encryption().RsaDecrypt(Convert.FromBase64String(encryptedPasswordDigest));

        Player = new Player {
            Id = EntityRegistry.FreeId,
            Username = username,
            Email = email,
            CountryCode = ClientSession.GetComponent<ClientLocaleComponent>().LocaleCode,
            HardwareFingerprint = hardwareFingerprint,
            Subscribed = subscribed,
            RegistrationTime = DateTimeOffset.UtcNow,
            LastLoginTime = DateTimeOffset.UtcNow,
            LastQuestUpdateTime = DateTimeOffset.UtcNow,
            LastLoginRewardTime = DateTimeOffset.Parse("2024-01-01 00:00:00"),
            PasswordHash = passwordHash
        };

        await using (DbConnection db = new()) {
            await db.InsertAsync(Player);

            if (Invite != null) {
                Invite.RemainingUses--;
                await db.UpdateAsync(Invite);
            }
        }

        await Player.InitializeNew();
        await Login(true, true, hardwareFingerprint);
    }

    public async Task Login(
        bool saveAutoLoginToken,
        bool rememberMe,
        string hardwareFingerprint) {
        Logger = Logger.WithPlayer((SocketPlayerConnection)this);

        Player.RememberMe = rememberMe;
        Player.LastLoginTime = DateTimeOffset.UtcNow;
        Player.HardwareFingerprint = hardwareFingerprint;

        if (saveAutoLoginToken) {
            Encryption encryption = new();

            byte[] autoLoginToken = new byte[32];
            new Random().NextBytes(autoLoginToken);

            byte[] encryptedAutoLoginToken = encryption.EncryptAutoLoginToken(autoLoginToken, Player.PasswordHash);

            Player.AutoLoginToken = autoLoginToken;
            await Send(new SaveAutoLoginTokenEvent(Player.Username, encryptedAutoLoginToken));
        }

        User = new UserTemplate().Create(Player);
        await Share(User);

        await ClientSession.AddComponentFrom<UserGroupComponent>(User);

        if (EntityRegistry.TryGetTemp(Player.Id, out IEntity? tempUser)) { // todo bugs
            foreach (IPlayerConnection connection in tempUser.SharedPlayers) {
                await connection.Unshare(tempUser);
                await connection.Share(User);
            }
        }

        Logger.Warning("Logged in");

        await using DbConnection db = new();
        await db.UpdateAsync(Player);
    }

    public async Task ChangePassword(string passwordDigest) {
        byte[] passwordHash = new Encryption().RsaDecrypt(Convert.FromBase64String(passwordDigest));
        Player.PasswordHash = passwordHash;

        await using DbConnection db = new();
        await db.Players
            .Where(player => player.Id == Player.Id)
            .Set(player => player.PasswordHash, Player.PasswordHash)
            .UpdateAsync();
    }

    public async Task ChangeReputation(int delta) {
        DateOnly date = DateOnly.FromDateTime(DateTime.Today);

        await using DbConnection db = new();
        await db.BeginTransactionAsync();

        SeasonStatistics seasonStats = await db.SeasonStatistics
            .SingleAsync(stats => stats.PlayerId == Player.Id &&
                                  stats.SeasonNumber == ConfigManager.ServerConfig.SeasonNumber);

        ReputationStatistics? reputationStats = await db.ReputationStatistics
            .SingleOrDefaultAsync(repStats => repStats.PlayerId == Player.Id &&
                                              repStats.Date == date);

        League oldLeagueIndex = Player.League;
        uint oldReputation = Player.Reputation;

        reputationStats ??= new ReputationStatistics {
            Player = Player,
            Date = date,
            SeasonNumber = ConfigManager.ServerConfig.SeasonNumber
        };

        uint reputation = (uint)Math.Clamp(oldReputation + delta, 0, 99999);

        Player.Reputation = reputation;
        seasonStats.Reputation = reputation;
        reputationStats.Reputation = reputation;

        await User.ChangeComponent<UserReputationComponent>(component => component.Reputation = reputation);

        if (oldLeagueIndex != Player.League) {
            await User.RemoveComponent<LeagueGroupComponent>();
            await User.AddComponentFrom<LeagueGroupComponent>(Player.LeagueEntity);
        }

        if (seasonStats.Reputation != oldReputation)
            await db.UpdateAsync(seasonStats);

        await db.InsertOrReplaceAsync(reputationStats);

        if ((Player.RewardedLeagues & Player.League) != Player.League) {
            Dictionary<IEntity, int> rewards = Leveling.GetFirstLeagueEntranceReward(Player.League);

            if (rewards.Count != 0) {
                foreach ((IEntity entity, int amount) in rewards)
                    await PurchaseItem(entity, amount, 0, false, false);

                IEntity rewardNotification = new LeagueFirstEntranceRewardPersistentNotificationTemplate().Create(rewards);
                await Share(rewardNotification);
            }

            Player.RewardedLeagues |= Player.League;
        }

        await db.UpdateAsync(Player);
        await db.CommitTransactionAsync();
    }

    public async Task ChangeExperience(int delta) {
        await using DbConnection db = new();
        await db.BeginTransactionAsync();

        await db.Players
            .Where(player => player.Id == Player.Id)
            .Set(player => player.Experience, player => player.Experience + delta)
            .UpdateAsync();

        await db.SeasonStatistics
            .Where(stats => stats.PlayerId == Player.Id &&
                            stats.SeasonNumber == ConfigManager.ServerConfig.SeasonNumber)
            .Set(stats => stats.ExperienceEarned, stats => stats.ExperienceEarned + delta)
            .UpdateAsync();

        await db.CommitTransactionAsync();
        Player.Experience += delta;
        await User.ChangeComponent<UserExperienceComponent>(component => component.Experience = Player.Experience);
    }

    public async Task ChangeGameplayChestScore(int delta) {
        const int scoreLimit = 1000;

        Player.GameplayChestScore += delta;
        int earned = (int)Math.Floor((double)Player.GameplayChestScore / scoreLimit);

        if (earned != 0) {
            Player.GameplayChestScore -= earned * scoreLimit;
            await PurchaseItem(Player.LeagueEntity.GetComponent<ChestBattleRewardComponent>().Chest, earned, 0, false, false);
        }

        try {
            await using DbConnection db = new();
            await db.UpdateAsync(Player);
        } catch (Exception e) {
            Logger.Error(e, "Failed to update gameplay chest score in database");
            return;
        }

        await User.ChangeComponent<GameplayChestScoreComponent>(component => component.Current = Player.GameplayChestScore);
    }

    public async Task CheckRankUp() {
        UserRankComponent rankComponent = User.GetComponent<UserRankComponent>();

        while (rankComponent.Rank < Player.Rank) {
            rankComponent.Rank++;
            await User.ChangeComponent(rankComponent);

            int rankIndex = rankComponent.Rank - 1;
            int crystals = CalculateCrystals(rankIndex);
            int xCrystals = CalculateXCrystals(rankIndex);

            CreateByRankConfigComponent createByRankConfigComponent = ConfigManager.GetComponent<CreateByRankConfigComponent>("garage/preset");

            if (createByRankConfigComponent.UserRankListToCreateItem.Contains(rankComponent.Rank))
                await PurchaseItem(GlobalEntities.GetEntity("misc", "Preset"), 1, 0, false, false);

            await ChangeCrystals(crystals);
            await ChangeXCrystals(xCrystals);
            await Share(new UserRankRewardNotificationTemplate().Create(rankComponent.Rank, crystals, xCrystals));
            BattlePlayer?.RankUp();
        }

        return;

        static int CalculateCrystals(int rankIndex) => rankIndex switch {
            < 9 => 100,
            < 12 => 500,
            < 14 => 1200,
            < 19 => 3000,
            < 49 => 3500,
            < 99 => 4000,
            _ => 7500
        };

        static int CalculateXCrystals(int rankIndex) =>
            rankIndex == 100 ? 100 :
            rankIndex % 10 == 0 ? 50 :
            rankIndex % 5 == 0 ? 20 :
            0;
    }

    public async Task PurchaseItem(IEntity marketItem, int amount, int price, bool forXCrystals, bool mount) {
        await using DbConnection db = new();
        IEntity? userItem = null;
        EntityTemplate? template = marketItem.TemplateAccessor?.Template;

        switch (template) {
            case AvatarMarketItemTemplate: {
                await db.InsertAsync(new Avatar { Player = Player, Id = marketItem.Id });
                break;
            }

            case GraffitiMarketItemTemplate:
            case ChildGraffitiMarketItemTemplate: {
                await db.InsertAsync(new Graffiti { Player = Player, Id = marketItem.Id });
                break;
            }

            case CrystalMarketItemTemplate: {
                await ChangeCrystals(amount);
                mount = false;
                break;
            }

            case XCrystalMarketItemTemplate: {
                await ChangeXCrystals(amount);
                mount = false;
                break;
            }

            case GoldBonusMarketItemTemplate: {
                await SetGoldBoxes(Player.GoldBoxItems + amount);
                mount = false;
                break;
            }

            case TankMarketItemTemplate: {
                long skinId = GlobalEntities.DefaultSkins[marketItem.Id];
                IEntity skin = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == skinId);

                await db.InsertAsync(new Hull { Player = Player, Id = marketItem.Id, SkinId = skinId });
                await PurchaseItem(skin, 1, 0, false, false);
                await MountItem(skin.GetUserEntity(this));
                break;
            }

            case WeaponMarketItemTemplate: {
                long skinId = GlobalEntities.DefaultSkins[marketItem.Id];
                long shellId = GlobalEntities.DefaultShells[marketItem.Id];

                IEntity skin = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == skinId);
                IEntity shell = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == shellId);

                await db.InsertAsync(new Weapon { Player = Player, Id = marketItem.Id, SkinId = skinId, ShellId = shellId });
                await PurchaseItem(skin, 1, 0, false, false);
                await PurchaseItem(shell, 1, 0, false, false);

                await MountItem(skin.GetUserEntity(this));
                await MountItem(shell.GetUserEntity(this));
                break;
            }

            case HullSkinMarketItemTemplate: {
                long hullId = marketItem.GetComponent<ParentGroupComponent>().Key;

                if (!await db.Hulls.AnyAsync(hull => hull.PlayerId == Player.Id && hull.Id == hullId)) return;

                await db.InsertAsync(new HullSkin { Player = Player, Id = marketItem.Id, HullId = hullId });
                break;
            }

            case WeaponSkinMarketItemTemplate: {
                long weaponId = marketItem.GetComponent<ParentGroupComponent>().Key;

                if (!await db.Weapons.AnyAsync(weapon => weapon.PlayerId == Player.Id && weapon.Id == weaponId)) return;

                await db.InsertAsync(new WeaponSkin { Player = Player, Id = marketItem.Id, WeaponId = weaponId });
                break;
            }

            case TankPaintMarketItemTemplate: {
                await db.InsertAsync(new Paint { Player = Player, Id = marketItem.Id });
                break;
            }

            case WeaponPaintMarketItemTemplate: {
                await db.InsertAsync(new Cover { Player = Player, Id = marketItem.Id });
                break;
            }

            case ShellMarketItemTemplate: {
                long weaponId = marketItem.GetComponent<ParentGroupComponent>().Key;

                if (!await db.Weapons.AnyAsync(weapon => weapon.PlayerId == Player.Id && weapon.Id == weaponId)) return;

                await db.InsertAsync(new Shell { Player = Player, Id = marketItem.Id, WeaponId = weaponId });
                break;
            }

            case ModuleCardMarketItemTemplate: {
                long moduleId = marketItem.GetComponent<ParentGroupComponent>().Key;
                Module? module = Player.Modules.SingleOrDefault(module => module.Id == moduleId);

                if (module == null) {
                    module = new Module { Player = Player, Id = moduleId };
                    Player.Modules.Add(module);
                }

                module.Cards += amount;
                await db.InsertOrReplaceAsync(module);
                break;
            }

            case DonutChestMarketItemTemplate:
            case GameplayChestMarketItemTemplate:
            case ContainerPackPriceMarketItemTemplate:
            case TutorialGameplayChestMarketItemTemplate: {
                Container? container = await db.Containers.SingleOrDefaultAsync(cont => cont.PlayerId == Player.Id && cont.Id == marketItem.Id);

                if (container == null) {
                    container = new Container { Player = Player, Id = marketItem.Id, Count = amount };
                    await db.InsertAsync(container);
                } else {
                    container.Count += amount;
                    await db.UpdateAsync(container);
                }

                mount = false;
                break;
            }

            case PremiumBoostMarketItemTemplate:
            case PremiumQuestMarketItemTemplate:
                Logger.Information("User purchased Premium Boost or Quest. Action is not implemented");
                break;

            case PresetMarketItemTemplate: {
                List<Preset> presets = await db.Presets.Where(preset => preset.PlayerId == Player.Id).ToListAsync();

                Preset preset = new() { Player = Player, Index = presets.Count, Name = $"Preset {presets.Count + 1}" };
                userItem = GlobalEntities.GetEntity("misc", "Preset");

                userItem.TemplateAccessor!.Template = ((MarketEntityTemplate)userItem.TemplateAccessor.Template).UserTemplate;
                userItem.Id = EntityRegistry.FreeId;

                await userItem.AddComponent(new PresetEquipmentComponent(preset));
                await userItem.AddComponent(new PresetNameComponent(preset));

                preset.Entity = userItem;
                Player.UserPresets.Add(preset);

                await db.InsertAsync(preset);
                await Share(userItem);
                break;
            }

            default:
                Logger.Error("{Name} purchase is not implemented", template?.GetType().Name);
                throw new NotImplementedException();
        }

        userItem ??= marketItem.GetUserEntity(this);
        await userItem.AddComponentIfAbsent(new UserGroupComponent(User));

        if (price > 0) {
            if (forXCrystals) await ChangeXCrystals(-price);
            else await ChangeCrystals(-price);
        }

        if (userItem.HasComponent<UserItemCounterComponent>()) {
            await userItem.ChangeComponent<UserItemCounterComponent>(component => component.Count += amount);
            await Send(new ItemsCountChangedEvent(amount), userItem);
        }

        if (mount) await MountItem(userItem);
    }

    public async Task MountItem(IEntity userItem) {
        bool changeEquipment = false;
        Preset currentPreset = Player.CurrentPreset;
        IEntity marketItem = userItem.GetMarketEntity(this);

        await using (DbConnection db = new()) {
            switch (userItem.TemplateAccessor!.Template) {
                case AvatarUserItemTemplate: {
                    await this.GetEntity(Player.CurrentAvatarId)!.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    await userItem.AddComponent<MountedItemComponent>();

                    Player.CurrentAvatarId = marketItem.Id;
                    await User.ChangeComponent(new UserAvatarComponent(this, Player.CurrentAvatarId));

                    await db.UpdateAsync(Player);
                    break;
                }

                case GraffitiUserItemTemplate: {
                    await currentPreset.Graffiti.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.Graffiti = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case TankUserItemTemplate: {
                    changeEquipment = true;
                    await currentPreset.Hull.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.Hull = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();
                    currentPreset.Entity!.GetComponent<PresetEquipmentComponent>().SetHullId(currentPreset.Hull.Id);

                    Hull newHull = await db.Hulls
                        .Where(hull => hull.PlayerId == Player.Id)
                        .SingleAsync(hull => hull.Id == currentPreset.Hull.Id);

                    IEntity skin = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == newHull.SkinId);

                    await currentPreset.HullSkin.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.HullSkin = skin;
                    await currentPreset.HullSkin.GetUserEntity(this).AddComponentIfAbsent<MountedItemComponent>();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case WeaponUserItemTemplate: {
                    changeEquipment = true;
                    await currentPreset.Weapon.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.Weapon = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();
                    currentPreset.Entity!.GetComponent<PresetEquipmentComponent>().SetWeaponId(currentPreset.Weapon.Id);

                    Weapon newWeapon = await db.Weapons
                        .Where(weapon => weapon.PlayerId == Player.Id)
                        .SingleAsync(weapon => weapon.Id == currentPreset.Weapon.Id);

                    IEntity skin = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == newWeapon.SkinId);
                    IEntity shell = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == newWeapon.ShellId);

                    await currentPreset.WeaponSkin.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.WeaponSkin = skin;
                    await currentPreset.WeaponSkin.GetUserEntity(this).AddComponentIfAbsent<MountedItemComponent>();

                    await currentPreset.Shell.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.Shell = shell;
                    await currentPreset.Shell.GetUserEntity(this).AddComponentIfAbsent<MountedItemComponent>();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case HullSkinUserItemTemplate: {
                    HullSkin skin = await db.HullSkins
                        .Where(skin => skin.PlayerId == Player.Id)
                        .SingleAsync(skin => skin.Id == marketItem.Id);

                    bool isCurrentHull = skin.HullId == currentPreset.Hull.Id;

                    if (!isCurrentHull) {
                        Hull? newHull = await db.Hulls.SingleOrDefaultAsync(hull => hull.PlayerId == Player.Id && hull.Id == skin.HullId);

                        if (newHull == null) return;

                        IEntity newUserHull = this.GetEntity(newHull.Id)!.GetUserEntity(this);
                        await MountItem(newUserHull);
                    }

                    await currentPreset.HullSkin.GetUserEntity(this).RemoveComponentIfPresent<MountedItemComponent>();
                    currentPreset.HullSkin = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();

                    await db.Hulls
                        .Where(hull => hull.PlayerId == Player.Id &&
                                       hull.Id == skin.HullId)
                        .Set(hull => hull.SkinId, skin.Id)
                        .UpdateAsync();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case WeaponSkinUserItemTemplate: {
                    WeaponSkin skin = await db.WeaponSkins
                        .Where(skin => skin.PlayerId == Player.Id)
                        .SingleAsync(skin => skin.Id == marketItem.Id);

                    bool isCurrentWeapon = skin.WeaponId == currentPreset.Weapon.Id;

                    if (!isCurrentWeapon) {
                        Weapon? newWeapon = await db.Weapons.SingleOrDefaultAsync(weapon => weapon.PlayerId == Player.Id && weapon.Id == skin.WeaponId);

                        if (newWeapon == null) return;

                        IEntity newUserWeapon = this.GetEntity(newWeapon.Id)!.GetUserEntity(this);
                        await MountItem(newUserWeapon);
                    }

                    await currentPreset.WeaponSkin.GetUserEntity(this).RemoveComponentIfPresent<MountedItemComponent>();
                    currentPreset.WeaponSkin = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();

                    await db.Weapons
                        .Where(weapon => weapon.PlayerId == Player.Id &&
                                         weapon.Id == currentPreset.Weapon.Id)
                        .Set(weapon => weapon.SkinId, currentPreset.WeaponSkin.Id)
                        .UpdateAsync();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case TankPaintUserItemTemplate: {
                    await currentPreset.Paint.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.Paint = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case WeaponPaintUserItemTemplate: {
                    await currentPreset.Cover.GetUserEntity(this).RemoveComponent<MountedItemComponent>();
                    currentPreset.Cover = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case ShellUserItemTemplate: {
                    Shell shell = await db.Shells
                        .Where(shell => shell.PlayerId == Player.Id)
                        .SingleAsync(shell => shell.Id == marketItem.Id);

                    bool isCurrentWeapon = shell.WeaponId == currentPreset.Weapon.Id;

                    if (!isCurrentWeapon) {
                        Weapon? newWeapon = await db.Weapons.SingleOrDefaultAsync(weapon => weapon.PlayerId == Player.Id && weapon.Id == shell.WeaponId);

                        if (newWeapon == null) return;

                        IEntity newUserWeapon = this.GetEntity(newWeapon.Id)!.GetUserEntity(this);
                        await MountItem(newUserWeapon);
                    }

                    await currentPreset.Shell.GetUserEntity(this).RemoveComponentIfPresent<MountedItemComponent>();
                    currentPreset.Shell = marketItem;
                    await userItem.AddComponent<MountedItemComponent>();

                    await db.Weapons
                        .Where(weapon => weapon.PlayerId == Player.Id &&
                                         weapon.Id == currentPreset.Weapon.Id)
                        .Set(weapon => weapon.ShellId, currentPreset.Shell.Id)
                        .UpdateAsync();

                    await db.UpdateAsync(currentPreset);
                    break;
                }

                case PresetUserItemTemplate: {
                    changeEquipment = true;
                    Preset? newPreset = Player.UserPresets.SingleOrDefault(preset => preset.Entity == userItem);

                    if (newPreset == null) return;

                    Dictionary<IEntity, IEntity> slotToCurrentModule = currentPreset.Modules.ToDictionary(
                        pModule => pModule.GetSlotEntity(this),
                        pModule => pModule.Entity.GetUserModule(this));

                    Dictionary<IEntity, IEntity> slotToNewModule = newPreset.Modules.ToDictionary(
                        pModule => pModule.GetSlotEntity(this),
                        pModule => pModule.Entity.GetUserModule(this));

                    foreach (IEntity entity in new[] {
                                 currentPreset.Hull.GetUserEntity(this),
                                 currentPreset.Paint.GetUserEntity(this),
                                 currentPreset.HullSkin.GetUserEntity(this),
                                 currentPreset.Weapon.GetUserEntity(this),
                                 currentPreset.Cover.GetUserEntity(this),
                                 currentPreset.WeaponSkin.GetUserEntity(this),
                                 currentPreset.Shell.GetUserEntity(this),
                                 currentPreset.Graffiti.GetUserEntity(this),
                                 currentPreset.Entity!
                             }) {
                        await entity.RemoveComponentIfPresent<MountedItemComponent>();
                    }

                    foreach (IEntity entity in new[] {
                                 newPreset.Hull.GetUserEntity(this),
                                 newPreset.Paint.GetUserEntity(this),
                                 newPreset.HullSkin.GetUserEntity(this),
                                 newPreset.Weapon.GetUserEntity(this),
                                 newPreset.Cover.GetUserEntity(this),
                                 newPreset.WeaponSkin.GetUserEntity(this),
                                 newPreset.Shell.GetUserEntity(this),
                                 newPreset.Graffiti.GetUserEntity(this),
                                 newPreset.Entity!
                             }) {
                        await entity.AddComponentIfAbsent<MountedItemComponent>();
                    }

                    foreach ((IEntity slot, IEntity module) in slotToCurrentModule) {
                        await slot.RemoveComponent<ModuleGroupComponent>();
                        await module.RemoveComponent<MountedItemComponent>();
                    }

                    foreach ((IEntity slot, IEntity module) in slotToNewModule) {
                        await slot.AddGroupComponent<ModuleGroupComponent>(module);
                        await module.AddComponent<MountedItemComponent>();
                    }

                    Player.CurrentPresetIndex = newPreset.Index;
                    await db.Players
                        .Where(player => player.Id == Player.Id)
                        .Set(player => player.CurrentPresetIndex, () => Player.CurrentPresetIndex)
                        .UpdateAsync();
                    break;
                }

                default: throw new NotImplementedException();
            }
        }

        if (!changeEquipment || !User.HasComponent<UserEquipmentComponent>()) return;

        await User.RemoveComponent<UserEquipmentComponent>();
        await User.AddComponent(new UserEquipmentComponent(Player.CurrentPreset.Weapon.Id, Player.CurrentPreset.Hull.Id));
    }

    public async Task AssembleModule(IEntity marketItem) {
        await using DbConnection db = new();
        Module? module = Player.Modules.SingleOrDefault(module => module.Id == marketItem.Id);

        if (module is not { Level: -1, Cards: > 0 }) {
            Logger.Error("Module {Id} is not ready to assemble", marketItem.Id);
            return;
        }

        module.Cards -= marketItem.GetComponent<ModuleCardsCompositionComponent>().CraftPrice.Cards;
        module.Level++;

        await db.UpdateAsync(module);

        IEntity card = SharedEntities.Single(entity =>
            entity.TemplateAccessor?.Template is ModuleCardUserItemTemplate &&
            entity.GetComponent<ParentGroupComponent>().Key == marketItem.Id);

        IEntity userItem = marketItem.GetUserModule(this);

        await card.ChangeComponent<UserItemCounterComponent>(component => component.Count = module.Cards);
        await userItem.ChangeComponent<ModuleUpgradeLevelComponent>(component => component.Level = module.Level);
        await userItem.AddGroupComponent<UserGroupComponent>(User);

        await Send(new ModuleAssembledEvent(), userItem);
    }

    public async Task UpgradeModule(IEntity userItem, bool forXCrystals) {
        long id = userItem.GetComponent<ParentGroupComponent>().Key;

        await using DbConnection db = new();

        Module? module = Player.Modules.SingleOrDefault(module => module.Id == id);
        ModuleCardsCompositionComponent compositionComponent = userItem.GetComponent<ModuleCardsCompositionComponent>();

        if (module == null || module.Level >= compositionComponent.UpgradePrices.Count) {
            Logger.Error("Module {Id} is not upgradable", id);
            return;
        }

        ModulePrice price = compositionComponent.UpgradePrices[module.Level];

        if (module.Cards < price.Cards) {
            Logger.Error("Not enough cards to upgrade module {Id}", id);
            return;
        }

        bool crystalsEnough = forXCrystals
                                  ? price.XCrystals <= Player.XCrystals
                                  : price.Crystals <= Player.Crystals;

        if (!crystalsEnough) {
            Logger.Error("Not enough (x)crystals to upgrade module {Id}", id);
            return;
        }

        if (forXCrystals) await ChangeXCrystals(-price.XCrystals);
        else await ChangeCrystals(-price.Crystals);

        module.Cards -= price.Cards;
        module.Level++;

        await db.UpdateAsync(module);

        IEntity card = SharedEntities.Single(entity =>
            entity.TemplateAccessor?.Template is ModuleCardUserItemTemplate &&
            entity.GetComponent<ParentGroupComponent>().Key == id);

        await card.ChangeComponent<UserItemCounterComponent>(component => component.Count = module.Cards);
        await userItem.ChangeComponent<ModuleUpgradeLevelComponent>(component => component.Level = module.Level);

        await Send(new ModuleUpgradedEvent(), userItem);
    }

    public async Task<bool> OwnsItem(IEntity marketItem) {
        await using DbConnection db = new();

        return marketItem.TemplateAccessor!.Template switch {
            AvatarMarketItemTemplate => await db.Avatars.AnyAsync(avatar => avatar.PlayerId == Player.Id && avatar.Id == marketItem.Id),
            TankMarketItemTemplate => await db.Hulls.AnyAsync(hull => hull.PlayerId == Player.Id && hull.Id == marketItem.Id),
            WeaponMarketItemTemplate => await db.Weapons.AnyAsync(weapon => weapon.PlayerId == Player.Id && weapon.Id == marketItem.Id),
            HullSkinMarketItemTemplate => await db.HullSkins.AnyAsync(hullSkin => hullSkin.PlayerId == Player.Id && hullSkin.Id == marketItem.Id),
            WeaponSkinMarketItemTemplate => await db.WeaponSkins.AnyAsync(weaponSkin => weaponSkin.PlayerId == Player.Id && weaponSkin.Id == marketItem.Id),
            TankPaintMarketItemTemplate => await db.Paints.AnyAsync(paint => paint.PlayerId == Player.Id && paint.Id == marketItem.Id),
            WeaponPaintMarketItemTemplate => await db.Covers.AnyAsync(cover => cover.PlayerId == Player.Id && cover.Id == marketItem.Id),
            ShellMarketItemTemplate => await db.Shells.AnyAsync(shell => shell.PlayerId == Player.Id && shell.Id == marketItem.Id),
            GraffitiMarketItemTemplate => await db.Graffities.AnyAsync(graffiti => graffiti.PlayerId == Player.Id && graffiti.Id == marketItem.Id),
            ChildGraffitiMarketItemTemplate => await db.Graffities.AnyAsync(graffiti => graffiti.PlayerId == Player.Id && graffiti.Id == marketItem.Id),
            ContainerPackPriceMarketItemTemplate => await db.Containers.AnyAsync(container => container.PlayerId == Player.Id && container.Id == marketItem.Id),
            DonutChestMarketItemTemplate => await db.Containers.AnyAsync(chest => chest.PlayerId == Player.Id && chest.Id == marketItem.Id),
            GameplayChestMarketItemTemplate => await db.Containers.AnyAsync(chest => chest.PlayerId == Player.Id && chest.Id == marketItem.Id),
            TutorialGameplayChestMarketItemTemplate => await db.Containers.AnyAsync(chest => chest.PlayerId == Player.Id && chest.Id == marketItem.Id),
            _ => false
        };
    }

    public virtual async Task SetUsername(string username) {
        Logger.Warning("Changed username => '{New}'", username);
        Player.Username = username;
        await User.ChangeComponent<UserUidComponent>(component => component.Username = username);

        await using DbConnection db = new();
        await db.Players
            .Where(player => player.Id == Player.Id)
            .Set(player => player.Username, username)
            .UpdateAsync();
    }

    public async Task ChangeCrystals(long delta) {
        if (delta == 0) return;

        await using DbConnection db = new();
        await db.BeginTransactionAsync();

        if (delta > 0) {
            await db.Statistics
                .Where(stats => stats.PlayerId == Player.Id)
                .Set(stats => stats.CrystalsEarned, stats => stats.CrystalsEarned + (ulong)delta)
                .UpdateAsync();

            await db.SeasonStatistics
                .Where(stats => stats.PlayerId == Player.Id && stats.SeasonNumber == ConfigManager.ServerConfig.SeasonNumber)
                .Set(stats => stats.CrystalsEarned, stats => stats.CrystalsEarned + (ulong)delta)
                .UpdateAsync();
        }

        await db.Players
            .Where(player => player.Id == Player.Id)
            .Set(player => player.Crystals, player => player.Crystals + delta)
            .UpdateAsync();

        await db.CommitTransactionAsync();
        Player.Crystals += delta;
        await User.ChangeComponent<UserMoneyComponent>(component => component.Money = Player.Crystals);
    }

    public async Task ChangeXCrystals(long delta) {
        await using DbConnection db = new();
        await db.BeginTransactionAsync();

        if (delta > 0) {
            await db.Statistics
                .Where(stats => stats.PlayerId == Player.Id)
                .Set(stats => stats.XCrystalsEarned, stats => stats.XCrystalsEarned + (ulong)delta)
                .UpdateAsync();

            await db.SeasonStatistics
                .Where(stats => stats.PlayerId == Player.Id && stats.SeasonNumber == ConfigManager.ServerConfig.SeasonNumber)
                .Set(stats => stats.XCrystalsEarned, stats => stats.XCrystalsEarned + (ulong)delta)
                .UpdateAsync();
        }

        await db.Players
            .Where(player => player.Id == Player.Id)
            .Set(player => player.XCrystals, player => player.XCrystals + delta)
            .UpdateAsync();

        await db.CommitTransactionAsync();
        Player.XCrystals += delta;
        await User.ChangeComponent<UserXCrystalsComponent>(component => component.Money = Player.XCrystals);
    }

    public async Task SetGoldBoxes(int goldBoxes) {
        await using DbConnection db = new();

        await db.Players
            .Where(player => player.Id == Player.Id)
            .Set(player => player.GoldBoxItems, goldBoxes)
            .UpdateAsync();

        Player.GoldBoxItems = goldBoxes;
    }

    public async Task DisplayMessage(string message, TimeSpan? closeTime = null) {
        IEntity notification = new SimpleTextNotificationTemplate().Create(message);

        await Share(notification);
        Schedule(closeTime ?? TimeSpan.FromSeconds(15), async () => await UnshareIfShared(notification));
    }

    public async Task SetClipboard(string content) {
        await Send(new SetClipboardEvent(content));
        await Share(new ClipboardSetNotificationTemplate().Create(User));
    }

    public ValueTask OpenURL(string url) => Send(new OpenURLEvent(url));

    public abstract Task Kick(string? reason);

    public abstract ValueTask Send(ICommand command);

    public ValueTask Send(IEvent @event) => Send(@event, ClientSession);

    public ValueTask Send(IEvent @event, params IEntity[] entities) => Send(new SendEventCommand(@event, entities));

    public Task Share(IEntity entity) => entity.Share(this);

    public async Task ShareIfUnshared(IEntity entity) {
        if (!SharedEntities.Contains(entity))
            await Share(entity);
    }

    public Task Unshare(IEntity entity) => entity.Unshare(this);

    public async Task UnshareIfShared(IEntity entity) {
        if (SharedEntities.Contains(entity))
            await Unshare(entity);
    }

    public void Schedule(TimeSpan delay, Action action) =>
        DelayedActions.Add(new DelayedAction(DateTimeOffset.UtcNow + delay, action));

    public void Schedule(DateTimeOffset time, Action action) =>
        DelayedActions.Add(new DelayedAction(time, action));

    public void Schedule(TimeSpan delay, Func<Task> action) =>
        DelayedTasks.Add(new DelayedTask(DateTimeOffset.UtcNow + delay, action));

    public void Schedule(DateTimeOffset time, Func<Task> action) =>
        DelayedTasks.Add(new DelayedTask(time, action));

    public virtual async Task Tick() {
        if (PingSendTime + TimeSpan.FromSeconds(5) <= DateTimeOffset.UtcNow) {
            await Send(new PingEvent(DateTimeOffset.UtcNow));
            PingSendTime = DateTimeOffset.UtcNow;
        }

        foreach (DelayedAction delayedAction in DelayedActions.Where(delayedAction => delayedAction.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            delayedAction.Action();
            DelayedActions.TryRemove(delayedAction);
        }

        foreach (DelayedTask delayedTask in DelayedTasks.Where(delayedTask => delayedTask.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            await delayedTask.Task();
            DelayedTasks.TryRemove(delayedTask);
        }
    }

    public override int GetHashCode() => Id.GetHashCode();

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public override string ToString() => $"PlayerConnection {{ " +
                                         $"ClientSession Id: '{ClientSession?.Id}'; " +
                                         $"Username: '{Player?.Username}' }}";
}

public class SocketPlayerConnection(
    GameServer server,
    Socket socket,
    Protocol.Protocol protocol
) : PlayerConnection(server) {
    public IPEndPoint EndPoint { get; } = (IPEndPoint)socket.RemoteEndPoint!;

    public override bool IsOnline => IsConnected && IsSocketConnected && ClientSession != null! && User != null! && Player != null!;
    public bool IsSocketConnected => Socket.Connected;
    bool IsConnected { get; set; }

    Socket Socket { get; } = socket;
    Protocol.Protocol Protocol { get; } = protocol;

    Channel<ICommand> ExecuteChannel { get; } =
        Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = true
        });

    Channel<ICommand> SendChannel { get; } =
        Channel.CreateUnbounded<ICommand>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = true
        });

    public override async Task SetUsername(string username) {
        await base.SetUsername(username);
        Logger = Logger.WithPlayer(this);
    }

    public override async Task Kick(string? reason) {
        Logger.Warning("Player kicked (reason: '{Reason}')", reason);
        await Disconnect();
    }

    public async Task OnConnected() {
        Logger = Logger.WithEndPoint(EndPoint);

        ClientSession = new ClientSessionTemplate().Create();
        Logger.Information("New socket connected");

        await Send(new InitTimeCommand(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        await Share(ClientSession);

        Extensions.RunTaskInBackground(ReceiveLoop, OnException, true);
        Extensions.RunTaskInBackground(SendLoop, OnException, true);
        Extensions.RunTaskInBackground(ExecuteLoop, OnException, true);

        IsConnected = true;
    }

    public override ValueTask Send(ICommand command) {
        if (!IsSocketConnected)
            return ValueTask.CompletedTask;

        Logger.Debug("Queueing for sending {Command}", command);
        return SendChannel.Writer.WriteAsync(command);
    }

    async Task Disconnect() {
        if (!IsConnected) return;

        try {
            Socket.Shutdown(SocketShutdown.Both);
        } finally {
            Socket.Close();
            await OnDisconnected();
        }
    }

    async Task OnDisconnected() {
        if (!IsConnected) return;

        IsConnected = false;
        Logger.Information("Socket disconnected");

        try {
            if (User != null!) {
                foreach (IPlayerConnection connection in User.SharedPlayers.Where(connection => !connection.InLobby)) {
                    try {
                        await connection.Unshare(User);
                    } catch { /**/ }
                }

                try {
                    EntityRegistry.Remove(User.Id);
                } catch (Exception e) {
                    Logger.Error(e, "User is already removed from registry");
                }

                foreach (DiscordLinkRequest request in ConfigManager.DiscordLinkRequests.Where(dLinkReq => dLinkReq.UserId == User.Id)) {
                    try {
                        ConfigManager.DiscordLinkRequests.TryRemove(request);
                    } catch { /**/ }
                }
            }

            if (!InLobby) return;

            if (BattlePlayer!.InBattleAsTank || BattlePlayer.IsSpectator)
                await BattlePlayer.Battle.RemovePlayer(BattlePlayer);
            else
                await BattlePlayer.Battle.RemovePlayerFromLobby(BattlePlayer);
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while disconnecting socket");
        } finally {
            Server.RemovePlayer(Id);

            SendChannel.Writer.TryComplete();
            ExecuteChannel.Writer.TryComplete();

            foreach (IEntity entity in SharedEntities) {
                entity.SharedPlayers.TryRemove(this);

                try {
                    if (entity.SharedPlayers.Count == 0 && !EntityRegistry.TryRemoveTemp(entity.Id))
                        EntityRegistry.Remove(entity.Id);
                } catch { /**/ }
            }

            SharedEntities.Clear();
        }
    }

    public override async Task Tick() {
        if (!IsSocketConnected) {
            await Kick("Zombie");
            return;
        }

        await base.Tick();
    }

    async Task ReceiveLoop() {
        byte[] bytes = ArrayPool<byte>.Shared.Rent(4096);

        try {
            while (IsSocketConnected) {
                ProtocolBuffer buffer = new(new OptionalMap(), this);
                await using NetworkStream stream = new(Socket);
                using BinaryReader reader = new BigEndianBinaryReader(stream);

                if (!buffer.Unwrap(reader))
                    throw new InvalidDataException("Failed to unwrap packet");

                long availableForRead = buffer.Stream.Length - buffer.Stream.Position;

                while (availableForRead > 0) {
                    Logger.Verbose("Decode buffer bytes available: {Count}", availableForRead);

                    ICommand command = (ICommand)Protocol.GetCodec(new TypeCodecInfo(typeof(ICommand))).Decode(buffer);

                    Logger.Debug("Queueing for executing: {Command}", command);
                    await ExecuteChannel.Writer.WriteAsync(command);

                    availableForRead = buffer.Stream.Length - buffer.Stream.Position;
                }

                Array.Clear(bytes);
            }
        } catch (Exception e) {
            Logger.Error(e, "Socket caught an exception in ReceiveLoop");
            await Disconnect();
        } finally {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    async Task SendLoop() {
        try {
            while (IsSocketConnected) {
                ICommand command = await SendChannel.Reader.ReadAsync();

                try {
                    Logger.Verbose("Encoding {Command}", command);

                    ProtocolBuffer buffer = new(new OptionalMap(), this);
                    Protocol.GetCodec(new TypeCodecInfo(typeof(ICommand))).Encode(buffer, command);

                    using MemoryStream stream = new();
                    await using BinaryWriter writer = new BigEndianBinaryWriter(stream);
                    buffer.Wrap(writer);

                    byte[] bytes = stream.ToArray();
                    await Socket.SendAsync(bytes);

                    Logger.Verbose("Sent {Command}: {Size} bytes ({Hex})", command, bytes.Length, Convert.ToHexString(bytes));
                } catch (Exception e) {
                    Logger.Error(e, "Failed to send {Command}", command);
                }
            }
        } catch (InvalidOperationException) { }
    }

    async Task ExecuteLoop() {
        try {
            while (true) {
                ICommand command = await ExecuteChannel.Reader.ReadAsync();

                try {
                    await command.Execute(this);
                } catch (Exception e) {
                    Logger.Error(e, "Failed to execute {Command}", command);
                }
            }
        } catch (Exception e) {
            Logger.Error(e, "Socket caught an exception in ExecuteLoop");
        }
    }

    async Task OnException(Exception e) {
        Logger.Error(e, "");
        await Disconnect();
    }
}
