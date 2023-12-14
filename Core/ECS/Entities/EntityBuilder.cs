using Vint.Core.ECS.Components;
using Vint.Core.ECS.Templates;

namespace Vint.Core.ECS.Entities;

public class EntityBuilder(
    long id
) : IEntityBuilder {
    public EntityBuilder() : this(EntityRegistry.FreeId) { }

    public long Id => id;
    public TemplateAccessor? TemplateAccessor { get; set; }
    public HashSet<IComponent> Components { get; } = new();

    public IEntityBuilder WithTemplateAccessor(TemplateAccessor templateAccessor) {
        TemplateAccessor = templateAccessor;
        return this;
    }

    public IEntityBuilder WithTemplateAccessor(EntityTemplate template, string? configPath) {
        TemplateAccessor = new TemplateAccessor(template, configPath);
        return this;
    }

    public IEntityBuilder AddComponent(IComponent component) {
        Components.Add(component);
        return this;
    }

    public IEntity Build() {
        Entity entity = new(Id, TemplateAccessor, Components.ToArray());
        EntityRegistry.Add(entity);

        return entity;
    }
}