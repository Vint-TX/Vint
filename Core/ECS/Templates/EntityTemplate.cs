using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Templates;

public abstract class EntityTemplate {
    protected IEntity Entity(string? configPath, Action<IEntityBuilder> builder) {
        EntityBuilder entityBuilder = new();

        entityBuilder.WithTemplateAccessor(this, configPath);
        builder(entityBuilder);

        return entityBuilder.Build();
    }
}

public abstract class MarketEntityTemplate : EntityTemplate {
    public abstract UserEntityTemplate UserTemplate { get; }
}

public abstract class UserEntityTemplate : EntityTemplate {
    public abstract MarketEntityTemplate MarketTemplate { get; }
}