using System.Diagnostics;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Fraction;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Modules;
using Vint.Core.ECS.Components.Preset;
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
        AllMarketTemplateEntities = ConfigManager.GetGlobalEntities().ToList(); // todo

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

    public static List<IEntity> GetEntities(this IPlayerConnection connection) {
        List<IEntity> entities = AllMarketTemplateEntities.ToList();
        entities.AddRange(GetUserEntities(connection));

        return entities;
    }

    public static IEnumerable<IEntity> GetUserTemplateEntities(this IPlayerConnection connection, string path) { // todo
        foreach (IEntity entity in GetEntities(path)) {
            Player player = connection.Player;
            IEntity user = connection.User;
            long entityId = entity.Id;

            entity.Id = EntityRegistry.FreeId;

            if (path == "moduleSlots") {
                entity.AddComponent(new UserGroupComponent(user));
                yield return entity;
            }

            if (entity.TemplateAccessor?.Template is not MarketEntityTemplate marketTemplate) continue; // yield return entity;??

            entity.TemplateAccessor.Template = marketTemplate.UserTemplate;

            using DbConnection db = new();

            switch (path) {
                case "avatars": {
                    if (db.Avatars.Any(avatar => avatar.PlayerId == player.Id && avatar.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));

                    break;
                }

                case "covers": {
                    if (db.Covers.Any(cover => cover.PlayerId == player.Id && cover.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));

                    break;
                }

                case "graffities": {
                    if (db.Graffities.Any(graffiti => graffiti.PlayerId == player.Id && graffiti.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));

                    break;
                }

                case "hulls": {
                    Hull? hull = db.Hulls.FirstOrDefault(hull => hull.PlayerId == player.Id && hull.Id == entityId);

                    if (hull != null)
                        entity.AddComponent(new UserGroupComponent(user));

                    long xp = hull?.Xp ?? 0;

                    entity.AddComponent(new ExperienceItemComponent(xp));
                    entity.AddComponent(new ExperienceToLevelUpItemComponent(xp));
                    entity.AddComponent(new UpgradeLevelItemComponent(xp));
                    entity.AddComponent(new UpgradeMaxLevelItemComponent());
                    break;
                }

                case "hullSkins": {
                    if (db.HullSkins.Any(hullSkin => hullSkin.PlayerId == player.Id && hullSkin.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));

                    break;
                }

                case "paints": {
                    if (db.Paints.Any(paint => paint.PlayerId == player.Id && paint.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));

                    break;
                }

                case "shells": {
                    if (db.Shells.Any(shell => shell.PlayerId == player.Id && shell.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));

                    break;
                }

                case "weapons": {
                    Weapon? weapon = db.Weapons.FirstOrDefault(weapon => weapon.PlayerId == player.Id && weapon.Id == entityId);

                    if (weapon != null)
                        entity.AddComponent(new UserGroupComponent(user));

                    long xp = weapon?.Xp ?? 0;

                    entity.AddComponent(new ExperienceItemComponent(xp));
                    entity.AddComponent(new ExperienceToLevelUpItemComponent(xp));
                    entity.AddComponent(new UpgradeLevelItemComponent(xp));
                    entity.AddComponent(new UpgradeMaxLevelItemComponent());
                    break;
                }

                case "weaponSkins": {
                    if (db.WeaponSkins.Any(weaponSkin => weaponSkin.PlayerId == player.Id && weaponSkin.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));

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

                    Module? module = db.Modules.SingleOrDefault(module => module.PlayerId == player.Id && module.Id == entityId);
                    int moduleLevel = module?.Level ?? 0;

                    if (moduleLevel > 0)
                        entity.AddComponent(new UserGroupComponent(user));

                    entity.AddComponent(new ModuleGroupComponent(entity));
                    entity.AddComponent(new ModuleUpgradeLevelComponent(moduleLevel));

                    break;
                }

                case "misc": {
                    entity.AddComponent(new UserGroupComponent(user));

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
                            foreach (Preset preset in db.Presets.Where(preset => preset.PlayerId == player.Id)) {
                                IEntity presetEntity = entity.Clone();
                                presetEntity.Id = EntityRegistry.FreeId;

                                presetEntity.AddComponent(new PresetEquipmentComponent(preset));
                                presetEntity.AddComponent(new PresetNameComponent(preset));

                                if (preset.Index == player.CurrentPresetIndex)
                                    presetEntity.AddComponent(new MountedItemComponent());

                                preset.Entity = presetEntity;
                                player.UserPresets.Add(preset);
                            }

                            connection.Share(player.UserPresets.Select(preset => preset.Entity!));
                            break;
                        }

                        case GoldBonusUserItemTemplate: {
                            IEntity gold = connection.UserEntities["modules"]
                                .Single(e => e.TemplateAccessor?.Template is GoldBonusModuleUserItemTemplate);

                            entity.AddComponent(new ModuleGroupComponent(gold));
                            entity.AddComponent(new UserItemCounterComponent(player.GoldBoxItems));

                            break;
                        }
                    }

                    break;
                }

                case "matchmakingModes": {
                    entity.AddComponent(new UserGroupComponent(user));
                    break;
                }

                case "containers": {
                    Container? container = db.Containers.SingleOrDefault(container => container.PlayerId == player.Id && container.Id == entityId);

                    if (entity.HasComponent<RestrictionByUserFractionComponent>())
                        entity.RemoveComponent<RestrictionByUserFractionComponent>();

                    entity.AddComponent(new UserGroupComponent(user));
                    entity.AddComponent(new NotificationGroupComponent(entity));
                    entity.AddComponent(new UserItemCounterComponent(container?.Count ?? 0));
                    break;
                }

                case "moduleCards": {
                    long moduleId = entity.GetComponent<ParentGroupComponent>().Key;
                    Module? module = db.Modules
                        .Where(module => module.PlayerId == player.Id)
                        .SingleOrDefault(module => module.Id == moduleId);

                    entity.AddComponent(new UserGroupComponent(user));
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
        predicate ??= entity => entity.GetComponent<MarketItemGroupComponent>().Key == userEntity.Id;

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

    static List<IEntity> GetUserEntities(this IPlayerConnection connection) {
        List<IEntity> entities = [];

        foreach (string entitiesTypeName in ConfigManager.GlobalEntitiesTypeNames) {
            List<IEntity> userEntities = GetUserTemplateEntities(connection, entitiesTypeName).ToList();

            connection.UserEntities[entitiesTypeName] = userEntities;
            entities.AddRange(userEntities);
        }

        return entities;
    }
}