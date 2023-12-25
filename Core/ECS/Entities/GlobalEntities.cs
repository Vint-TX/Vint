using System.Collections.ObjectModel;
using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Preset;
using Vint.Core.ECS.Templates;
using Vint.Core.ECS.Templates.Preset;
using Vint.Core.Server;

namespace Vint.Core.ECS.Entities;

public static class GlobalEntities {
    static GlobalEntities() =>
        AllMarketTemplateEntities = ConfigManager.GetGlobalEntities().ToList(); // todo

    public static List<IEntity> AllMarketTemplateEntities { get; private set; }

    public static List<IEntity> GetEntities(this IPlayerConnection connection) {
        List<IEntity> entities = AllMarketTemplateEntities.ToList();
        entities.AddRange(GetUserEntities(connection));

        return entities;
    }

    public static IEnumerable<IEntity> GetUserTemplateEntities(this IPlayerConnection connection, string path) { // todo
        foreach (IEntity entity in GetEntities(path)) {
            if (entity.TemplateAccessor?.Template is not MarketEntityTemplate marketTemplate) continue;

            long entityId = entity.Id;

            entity.Id = EntityRegistry.FreeId;
            entity.TemplateAccessor.Template = marketTemplate.UserTemplate;

            Player player = connection.Player;
            IEntity user = connection.User;
            
            switch (path) {
                case "avatars": {
                    if (player.Avatars.Exists(avatar => avatar.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));
                    
                    break;
                }

                case "covers": {
                    if (player.Covers.Exists(cover => cover.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));
                    
                    break;
                }

                case "graffities": {
                    if (player.Graffities.Exists(graffiti => graffiti.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));
                    
                    break;
                }

                case "hulls": {
                    Hull? hull = player.Hulls.FirstOrDefault(hull => hull.Id == entityId);
                    
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
                    if (player.HullSkins.Exists(hullSkin => hullSkin.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));
                    
                    break;
                }

                case "paints": {
                    if (player.Paints.Exists(paint => paint.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));
                    
                    break;
                }

                case "shells": {
                    if (player.Shells.Exists(shell => shell.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));
                    
                    break;
                }

                case "weapons": {
                    Weapon? weapon = player.Weapons.FirstOrDefault(weapon => weapon.Id == entityId);
                    
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
                    if (player.WeaponSkins.Exists(weaponSkin => weaponSkin.Id == entityId))
                        entity.AddComponent(new UserGroupComponent(user));
                    
                    break;
                }

                case "misc": { // todo
                    entity.AddComponent(new UserGroupComponent(user));

                    if (entity.TemplateAccessor.Template.GetType() == typeof(PresetUserItemTemplate)) {
                        foreach (Preset preset in player.Presets) {
                            IEntity presetEntity = entity.Clone();
                            presetEntity.Id = EntityRegistry.FreeId;
                            
                            presetEntity.AddComponent(new PresetEquipmentComponent(preset));
                            presetEntity.AddComponent(new PresetNameComponent(preset));
                            
                            if (preset.Index == player.CurrentPresetIndex)
                                presetEntity.AddComponent(new MountedItemComponent());

                            preset.Entity = presetEntity;
                        }
                        
                        connection.Share(player.Presets.Select(preset => preset.Entity));
                    }
                    break;
                }
            }

            yield return entity;
        }
    }

    public static IEntity? GetUserTemplateEntity(this IPlayerConnection connection, string path, string entityName) { // todo
        IEntity entity = GetEntity(path, entityName);

        if (entity.TemplateAccessor?.Template is not MarketEntityTemplate marketTemplate) return null;

        long entityId = entity.Id;

        entity.Id = EntityRegistry.FreeId;
        entity.TemplateAccessor.Template = marketTemplate.UserTemplate;

        return entity;
    }

    public static IEntity GetEntity(string typeName, string entityName) =>
        ConfigManager.GetGlobalEntity(typeName, entityName);

    public static IEnumerable<IEntity> GetEntities(string typeName) => ConfigManager.GetGlobalEntities(typeName);

    public static IEntity GetUserEntity(this IEntity marketEntity, IPlayerConnection connection) =>
        marketEntity.TemplateAccessor!.Template switch {
            UserEntityTemplate => marketEntity,
            MarketEntityTemplate marketTemplate => connection.SharedEntities.Single(entity =>
                entity.TemplateAccessor?.Template.GetType() == marketTemplate.UserTemplate.GetType() &&
                entity.GetComponent<MarketItemGroupComponent>().Key == marketEntity.Id),
            _ => throw new KeyNotFoundException()
        };

    public static IEntity GetMarketEntity(this IEntity userEntity, IPlayerConnection connection) =>
        userEntity.TemplateAccessor!.Template switch {
            MarketEntityTemplate => userEntity,
            UserEntityTemplate userTemplate => connection.SharedEntities.Single(entity =>
                entity.TemplateAccessor?.Template.GetType() == userTemplate.MarketTemplate.GetType() &&
                entity.GetComponent<MarketItemGroupComponent>().Key == userEntity.Id),
            _ => throw new KeyNotFoundException()
        };

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
}
