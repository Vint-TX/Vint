using Vint.Core.Config;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Templates;
using Vint.Core.Server;

namespace Vint.Core.ECS.Entities;

public static class GlobalEntities {
    static GlobalEntities() =>
        AllMarketTemplateEntities = ConfigManager.GetGlobalEntities().ToList(); // todo

    public static List<IEntity> AllMarketTemplateEntities { get; private set; }

    public static List<IEntity> GetEntities(this IPlayerConnection connection) {
        List<IEntity> entities = GetUserEntities(connection).ToList();
        entities.AddRange(AllMarketTemplateEntities);

        return entities;
    }

    public static IEnumerable<IEntity> GetUserTemplateEntities(this IPlayerConnection connection, string path) { // todo
        foreach (IEntity entity in GetEntities(path)) {
            if (entity.TemplateAccessor?.Template is not MarketEntityTemplate marketTemplate) continue;

            long entityId = entity.Id;

            entity.Id = EntityRegistry.FreeId;
            entity.TemplateAccessor.Template = marketTemplate.UserTemplate;

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

    public static IEntity GetUserEntity(this IPlayerConnection connection, IEntity marketEntity) =>
        marketEntity.TemplateAccessor!.Template switch {
            UserEntityTemplate => marketEntity,
            MarketEntityTemplate marketTemplate => connection.SharedEntities.Single(entity =>
                entity.TemplateAccessor?.Template.GetType() == marketTemplate.UserTemplate.GetType() &&
                entity.GetComponent<MarketItemGroupComponent>().Key == marketEntity.Id),
            _ => throw new KeyNotFoundException()
        };

    public static IEntity GetMarketEntity(this IPlayerConnection connection, IEntity userEntity) =>
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
}
