using System.Diagnostics;
using LinqToDB;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Fraction;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.Preset;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates;
using Vint.Core.ECS.Templates.Gold;
using Vint.Core.ECS.Templates.Modules;
using Vint.Core.ECS.Templates.Money;
using Vint.Core.ECS.Templates.Premium;
using Vint.Core.ECS.Templates.Preset;
using Vint.Core.Server;

namespace Vint.Core.ECS.Entities;

public static class GlobalEntities {
    static GlobalEntities() =>
        AllMarketTemplateEntities = ConfigManager.GetGlobalEntities().ToList();

    public static List<IEntity> AllMarketTemplateEntities { get; private set; }

    public static IReadOnlyDictionary<long, long> DefaultSkins { get; } = new Dictionary<long, long> {
        { GetEntity("weapons", "Flamethrower").Id, GetEntity("weaponSkins", "FlamethrowerM0").Id },
        { GetEntity("weapons", "Freeze").Id, GetEntity("weaponSkins", "FreezeM0").Id },
        { GetEntity("weapons", "Hammer").Id, GetEntity("weaponSkins", "HammerM0").Id },
        { GetEntity("weapons", "Isis").Id, GetEntity("weaponSkins", "IsisM0").Id },
        { GetEntity("weapons", "Railgun").Id, GetEntity("weaponSkins", "RailgunM0").Id },
        { GetEntity("weapons", "Ricochet").Id, GetEntity("weaponSkins", "RicochetM0").Id },
        { GetEntity("weapons", "Shaft").Id, GetEntity("weaponSkins", "ShaftM0").Id },
        { GetEntity("weapons", "Smoky").Id, GetEntity("weaponSkins", "SmokyM0").Id },
        { GetEntity("weapons", "Thunder").Id, GetEntity("weaponSkins", "ThunderM0").Id },
        { GetEntity("weapons", "Twins").Id, GetEntity("weaponSkins", "TwinsM0").Id },
        { GetEntity("weapons", "Vulcan").Id, GetEntity("weaponSkins", "VulcanM0").Id },

        { GetEntity("hulls", "Dictator").Id, GetEntity("hullSkins", "DictatorM0").Id },
        { GetEntity("hulls", "Hornet").Id, GetEntity("hullSkins", "HornetM0").Id },
        { GetEntity("hulls", "Hunter").Id, GetEntity("hullSkins", "HunterM0").Id },
        { GetEntity("hulls", "Mammoth").Id, GetEntity("hullSkins", "MammothM0").Id },
        { GetEntity("hulls", "Titan").Id, GetEntity("hullSkins", "TitanM0").Id },
        { GetEntity("hulls", "Viking").Id, GetEntity("hullSkins", "VikingM0").Id },
        { GetEntity("hulls", "Wasp").Id, GetEntity("hullSkins", "WaspM0").Id }
    };

    public static IReadOnlyDictionary<long, long> DefaultShells { get; } = new Dictionary<long, long> {
        { GetEntity("weapons", "Flamethrower").Id, GetEntity("shells", "FlamethrowerOrange").Id },
        { GetEntity("weapons", "Freeze").Id, GetEntity("shells", "FreezeSkyblue").Id },
        { GetEntity("weapons", "Hammer").Id, GetEntity("shells", "HammerStandard").Id },
        { GetEntity("weapons", "Isis").Id, GetEntity("shells", "IsisStandard").Id },
        { GetEntity("weapons", "Railgun").Id, GetEntity("shells", "RailgunPaleblue").Id },
        { GetEntity("weapons", "Ricochet").Id, GetEntity("shells", "RicochetAurulent").Id },
        { GetEntity("weapons", "Shaft").Id, GetEntity("shells", "ShaftStandard").Id },
        { GetEntity("weapons", "Smoky").Id, GetEntity("shells", "SmokyStandard").Id },
        { GetEntity("weapons", "Thunder").Id, GetEntity("shells", "ThunderStandard").Id },
        { GetEntity("weapons", "Twins").Id, GetEntity("shells", "TwinsBlue").Id },
        { GetEntity("weapons", "Vulcan").Id, GetEntity("shells", "VulcanStandard").Id }
    };

    public static IReadOnlyList<IEntity> Tier1Modules { get; } = new List<IEntity> {
        GetEntity("moduleCards", "Mine"),
        GetEntity("moduleCards", "Emp"),
        GetEntity("moduleCards", "Sonar"),
        GetEntity("moduleCards", "Engineer"),
        GetEntity("moduleCards", "BackhitIncrease"),
        GetEntity("moduleCards", "Rage"),
        GetEntity("moduleCards", "RepairKit"),
        GetEntity("moduleCards", "AbsorbingArmor"),
        GetEntity("moduleCards", "TurboSpeed"),
        GetEntity("moduleCards", "TempBlock"),
        GetEntity("moduleCards", "BackhitDefence")
    };

    public static IReadOnlyList<IEntity> Tier2Modules { get; } = new List<IEntity> {
        GetEntity("moduleCards", "SpiderMine"),
        GetEntity("moduleCards", "IncreasedDamage"),
        GetEntity("moduleCards", "ExternalImpact"),
        GetEntity("moduleCards", "Adrenaline"),
        GetEntity("moduleCards", "Kamikadze"),
        GetEntity("moduleCards", "ForceField"),
        GetEntity("moduleCards", "Invisibility"),
        GetEntity("moduleCards", "JumpImpact"),
        GetEntity("moduleCards", "AcceleratedGears"),
        GetEntity("moduleCards", "Sapper")
    };

    public static IReadOnlyList<IEntity> Tier3Modules { get; } = new List<IEntity> {
        GetEntity("moduleCards", "Drone"),
        GetEntity("moduleCards", "EnergyInjection"),
        GetEntity("moduleCards", "ExplosiveMass"),
        GetEntity("moduleCards", "LifeSteal"),
        GetEntity("moduleCards", "IceTrap"),
        GetEntity("moduleCards", "Invulnerability"),
        GetEntity("moduleCards", "FireRing"),
        GetEntity("moduleCards", "EmergencyProtection")
    };

    public static IEnumerable<IEntity> GetEntities(this IPlayerConnection connection) =>
        AllMarketTemplateEntities.Concat(GetUserEntities(connection));

    static IEnumerable<IEntity> GetUserTemplateEntities(this IPlayerConnection connection, string path) {
        foreach (IEntity entity in GetEntities(path)) {
            Player player = connection.Player;
            IEntity user = connection.User;
            long entityId = entity.Id;

            entity.Id = EntityRegistry.FreeId;

            if (path == "moduleSlots") {
                entity.AddGroupComponent<UserGroupComponent>(user);
                yield return entity;
            }

            if (entity.TemplateAccessor?.Template is not MarketEntityTemplate marketTemplate) continue;

            entity.TemplateAccessor.Template = marketTemplate.UserTemplate;

            using DbConnection db = new();

            switch (path) {
                case "avatars": {
                    if (db.Avatars.Any(avatar => avatar.PlayerId == player.Id && avatar.Id == entityId))
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    break;
                }

                case "covers": {
                    if (db.Covers.Any(cover => cover.PlayerId == player.Id && cover.Id == entityId))
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    break;
                }

                case "graffities": {
                    if (db.Graffities.Any(graffiti => graffiti.PlayerId == player.Id && graffiti.Id == entityId))
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    break;
                }

                case "hulls": {
                    Hull? hull = db.Hulls.FirstOrDefault(hull => hull.PlayerId == player.Id && hull.Id == entityId);

                    if (hull != null)
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    long xp = hull?.Xp ?? 0;

                    entity.AddComponent(new ExperienceItemComponent(xp));
                    entity.AddComponent(new ExperienceToLevelUpItemComponent(xp));
                    entity.AddComponent(new UpgradeLevelItemComponent(xp));
                    entity.AddComponent<UpgradeMaxLevelItemComponent>();
                    break;
                }

                case "hullSkins": {
                    if (db.HullSkins.Any(hullSkin => hullSkin.PlayerId == player.Id && hullSkin.Id == entityId))
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    break;
                }

                case "paints": {
                    if (db.Paints.Any(paint => paint.PlayerId == player.Id && paint.Id == entityId))
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    break;
                }

                case "shells": {
                    if (db.Shells.Any(shell => shell.PlayerId == player.Id && shell.Id == entityId))
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    break;
                }

                case "weapons": {
                    Weapon? weapon = db.Weapons.FirstOrDefault(weapon => weapon.PlayerId == player.Id && weapon.Id == entityId);

                    if (weapon != null)
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    long xp = weapon?.Xp ?? 0;

                    entity.AddComponent(new ExperienceItemComponent(xp));
                    entity.AddComponent(new ExperienceToLevelUpItemComponent(xp));
                    entity.AddComponent(new UpgradeLevelItemComponent(xp));
                    entity.AddComponent<UpgradeMaxLevelItemComponent>();
                    break;
                }

                case "weaponSkins": {
                    if (db.WeaponSkins.Any(weaponSkin => weaponSkin.PlayerId == player.Id && weaponSkin.Id == entityId))
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    break;
                }

                case "modules": {
                    ModuleBehaviourType moduleBehaviourType = entity.GetComponent<ModuleBehaviourTypeComponent>().BehaviourType;
                    string[] configPathParts = entity.TemplateAccessor.ConfigPath!.Split('/');

                    switch (moduleBehaviourType) {
                        case ModuleBehaviourType.Active: {
                            if (configPathParts[3] == "common") {
                                entity.TemplateAccessor.Template = new GoldBonusModuleUserItemTemplate();
                                break;
                            }

                            entity.TemplateAccessor.Template = new ActiveModuleUserItemTemplate();
                            break;
                        }

                        case ModuleBehaviourType.Passive: {
                            if (configPathParts[4] == "trigger") {
                                entity.TemplateAccessor.Template = new TriggerModuleUserItemTemplate();
                                break;
                            }

                            entity.TemplateAccessor.Template = new PassiveModuleUserItemTemplate();
                            break;
                        }

                        default: throw new UnreachableException();
                    }

                    Module? module = player.Modules.SingleOrDefault(module => module.Id == entityId);
                    int moduleLevel = module?.Level ?? -1;

                    if (moduleLevel >= 0)
                        entity.AddGroupComponent<UserGroupComponent>(user);

                    entity.AddGroupComponent<ModuleGroupComponent>();
                    entity.AddComponent(new ModuleUpgradeLevelComponent(moduleLevel));
                    break;
                }

                case "misc": {
                    entity.AddGroupComponent<UserGroupComponent>(user);

                    switch (entity.TemplateAccessor.Template) {
                        case PremiumBoostUserItemTemplate:
                        case PremiumQuestUserItemTemplate: {
                            entity.AddComponent(new DurationUserItemComponent());
                            break;
                        }

                        case CrystalUserItemTemplate: {
                            entity.AddComponent(new UserItemCounterComponent(player.Crystals));
                            break;
                        }

                        case XCrystalUserItemTemplate: {
                            entity.AddComponent(new UserItemCounterComponent(player.XCrystals));
                            break;
                        }

                        case PresetUserItemTemplate: {
                            foreach (Preset preset in db.Presets
                                         .LoadWith(preset => preset.Modules)
                                         .Where(preset => preset.PlayerId == player.Id)) {
                                IEntity presetEntity = entity.Clone();
                                presetEntity.Id = EntityRegistry.FreeId;

                                presetEntity.AddComponent(new PresetEquipmentComponent(preset));
                                presetEntity.AddComponent(new PresetNameComponent(preset));

                                if (preset.Index == player.CurrentPresetIndex)
                                    presetEntity.AddComponent<MountedItemComponent>();

                                preset.Entity = presetEntity;
                                player.UserPresets.Add(preset);
                                connection.Share(preset.Entity);
                            }

                            continue;
                        }

                        case GoldBonusUserItemTemplate: {
                            IEntity gold = connection.SharedEntities.Single(e => e.TemplateAccessor?.Template is GoldBonusModuleUserItemTemplate);

                            entity.AddGroupComponent<ModuleGroupComponent>(gold);
                            entity.AddComponent(new UserItemCounterComponent(player.GoldBoxItems));
                            break;
                        }
                    }

                    break;
                }

                case "matchmakingModes": {
                    entity.AddGroupComponent<UserGroupComponent>(user);
                    break;
                }

                case "containers": {
                    Container? container = db.Containers.SingleOrDefault(container => container.PlayerId == player.Id && container.Id == entityId);

                    entity.AddGroupComponent<UserGroupComponent>(user);
                    entity.AddComponent(new UserItemCounterComponent(container?.Count ?? 0));
                    entity.AddGroupComponent<NotificationGroupComponent>();
                    entity.RemoveComponentIfPresent<RestrictionByUserFractionComponent>();
                    break;
                }

                case "moduleCards": {
                    long moduleId = entity.GetComponent<ParentGroupComponent>().Key;
                    Module? module = player.Modules.SingleOrDefault(module => module.Id == moduleId);

                    entity.AddGroupComponent<UserGroupComponent>(user);
                    entity.AddComponent(new UserItemCounterComponent(module?.Cards ?? 0));
                    break;
                }
            }

            yield return entity;
        }
    }

    public static IEntity GetEntity(string typeName, string entityName) =>
        ConfigManager.GetGlobalEntity(typeName, entityName);

    public static IEnumerable<IEntity> GetEntities(string typeName) => ConfigManager.GetGlobalEntities(typeName);

    public static IEntity GetUserModule(this IEntity marketEntity, IPlayerConnection connection) =>
        connection.SharedEntities.Single(entity => entity.TemplateAccessor?.Template is UserEntityTemplate &&
                                                   entity.GetComponent<MarketItemGroupComponent>().Key == marketEntity.Id);

    public static IEntity GetUserEntity(this IEntity marketEntity, IPlayerConnection connection, Func<IEntity, bool>? predicate = null) {
        predicate ??= entity => entity.GetComponent<MarketItemGroupComponent>().Key == marketEntity.Id;

        return marketEntity.TemplateAccessor!.Template switch {
            UserEntityTemplate => marketEntity,
            MarketEntityTemplate marketTemplate => connection.SharedEntities.Single(entity =>
                entity.TemplateAccessor?.Template == marketTemplate.UserTemplate &&
                predicate(entity)),
            _ => throw new KeyNotFoundException()
        };
    }

    public static IEntity GetMarketEntity(this IEntity userEntity, IPlayerConnection connection, Func<IEntity, bool>? predicate = null) {
        predicate ??= entity => entity.Id == userEntity.GetComponent<MarketItemGroupComponent>().Key;

        return userEntity.TemplateAccessor!.Template switch {
            MarketEntityTemplate => userEntity,
            UserEntityTemplate userTemplate => connection.SharedEntities.Single(entity =>
                entity.TemplateAccessor?.Template == userTemplate.MarketTemplate &&
                predicate(entity)),
            _ => throw new KeyNotFoundException()
        };
    }

    public static IEntity? GetEntity(this IPlayerConnection connection, long entityId) =>
        connection.SharedEntities.SingleOrDefault(entity => entity.Id == entityId);

    public static async Task<bool> ValidatePurchase(IPlayerConnection connection, IEntity item, int amount, int price, bool forXCrystals) {
        string configPath = item.TemplateAccessor!.ConfigPath!;
        int? configPrice = null;

        if (ConfigManager.TryGetComponent(configPath, out PackPriceComponent? packPriceComponent)) {
            Dictionary<int, int> packPrice = forXCrystals
                                                 ? packPriceComponent.PackXPrice
                                                 : packPriceComponent.PackPrice;

            if (!packPrice.TryGetValue(amount, out int value)) return false;

            configPrice = value;
        } else {
            if (forXCrystals) {
                if (ConfigManager.TryGetComponent(configPath, out XPriceItemComponent? xPriceItemComponent))
                    configPrice = xPriceItemComponent.Price;
            } else if (ConfigManager.TryGetComponent(configPath, out PriceItemComponent? priceItemComponent))
                configPrice = priceItemComponent.Price;
            else return false;
        }

        if (item.TemplateAccessor?.Template is PresetMarketItemTemplate) {
            ItemsBuyCountLimitComponent buyCountLimitComponent = ConfigManager.GetComponent<ItemsBuyCountLimitComponent>(configPath);

            await using DbConnection db = new();
            int count = await db.Presets.CountAsync(preset => preset.PlayerId == connection.Player.Id);

            if (count >= buyCountLimitComponent.Limit) return false;

            /*if (count == 1) {
                FirstBuySaleComponent firstBuySaleComponent = ConfigManager.GetComponent<FirstBuySaleComponent>(configPath);
                configPrice -= configPrice * firstBuySaleComponent.SalePercent / 100;
            }*/

            ItemsAutoIncreasePriceComponent increasePriceComponent = ConfigManager.GetComponent<ItemsAutoIncreasePriceComponent>(configPath);

            configPrice += CalculateAdditionalPrice(increasePriceComponent, count);
        }

        if (configPrice != price) return false;

        bool crystalsEnough = (forXCrystals ? connection.Player.XCrystals : connection.Player.Crystals) >= price;

        return crystalsEnough && (forXCrystals ||
                                  !ConfigManager.TryGetComponent(configPath,
                                      out CrystalsPurchaseUserRankRestrictionComponent? restrictionComponent) ||
                                  connection.Player.Rank >= restrictionComponent.RestrictionValue);
    }

    static IEnumerable<IEntity> GetUserEntities(this IPlayerConnection connection) =>
        ConfigManager.GlobalEntitiesTypeNames
            .SelectMany(entitiesTypeName => GetUserTemplateEntities(connection, entitiesTypeName));

    static int CalculateAdditionalPrice(ItemsAutoIncreasePriceComponent increasePrice, int itemCount) {
        itemCount++;

        if (itemCount <= increasePrice.StartCount)
            return 0;

        long num = itemCount - increasePrice.StartCount;
        int num2 = (int)num * increasePrice.PriceIncreaseAmount;
        int maxAdditionalPrice = increasePrice.MaxAdditionalPrice;

        if (maxAdditionalPrice <= 0 || num2 < maxAdditionalPrice)
            return num2;

        return maxAdditionalPrice;
    }
}
