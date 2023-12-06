using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Templates;

public interface IEntityTemplate {
    public sealed IEntity Entity(string? configPath, Action<IEntityBuilder> builder) {
        EntityBuilder entityBuilder = new();

        entityBuilder.WithTemplateAccessor(this, configPath);
        builder(entityBuilder);

        return entityBuilder.Build();
    }
}