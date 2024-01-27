using Vint.Core.ECS.Components;
using Vint.Core.ECS.Templates;

namespace Vint.Core.ECS.Entities;

public class EntityBuilder() : IEntityBuilder {
    public EntityBuilder(long id) : this() => Id = id;

    public long Id { get; private set; }
    public TemplateAccessor? TemplateAccessor { get; private set; }
    public HashSet<IComponent> Components { get; } = [];

    public IEntityBuilder WithTemplateAccessor(TemplateAccessor templateAccessor) {
        TemplateAccessor = templateAccessor;
        return this;
    }

    public IEntityBuilder WithTemplateAccessor(EntityTemplate template, string? configPath) {
        TemplateAccessor = new TemplateAccessor(template, configPath);
        return this;
    }

    public IEntityBuilder WithId(long id) {
        Id = id;
        return this;
    }

    public IEntityBuilder AddComponent(IComponent component) {
        Components.Add(component);
        return this;
    }

    public IEntity Build(bool temp) {
        if (Id == 0)
            Id = EntityRegistry.FreeId;

        Entity entity = new(Id, TemplateAccessor, Components.ToList());
        if (!temp) EntityRegistry.Add(entity);

        return entity;
    }
}