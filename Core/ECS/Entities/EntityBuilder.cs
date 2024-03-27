using Vint.Core.Config;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Templates;

namespace Vint.Core.ECS.Entities;

public class EntityBuilder() : IEntityBuilder {
    public EntityBuilder(long id) : this() => Id = id;

    List<(Action<IEntity> action, Func<IEntity, bool> condition)> Actions { get; } = [];

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

    public IEntityBuilder AddComponent<T>() where T : class, IComponent, new() => AddComponent(new T());

    public IEntityBuilder AddComponent<T>(string configPath) where T : class, IComponent =>
        AddComponent(ConfigManager.GetComponent<T>(configPath));

    public IEntityBuilder AddGroupComponent<T>(IEntity? entity = null) where T : GroupComponent =>
        ThenExecuteIf(_ => true, thisEntity => thisEntity.AddGroupComponent<T>(entity));

    public IEntityBuilder AddComponentFrom<T>(IEntity entity) where T : class, IComponent {
        Components.Add(entity.GetComponent<T>());
        return this;
    }

    public IEntityBuilder TryAddComponent<T>(IEntity entity) where T : class, IComponent =>
        entity.HasComponent<T>() ? AddComponentFrom<T>(entity) : this;

    public IEntityBuilder TryAddComponent<T>(string configPath) where T : class, IComponent =>
        ConfigManager.TryGetComponent(configPath, out T? component) ? AddComponent(component) : this;

    public IEntityBuilder ThenExecuteIf(Func<IEntity, bool> condition, Action<IEntity> action) {
        Actions.Add((action, condition));
        return this;
    }

    public IEntity Build(bool temp) {
        if (Id == 0)
            Id = EntityRegistry.FreeId;

        Entity entity = new(Id, TemplateAccessor, Components.ToList());

        foreach (Action<IEntity> action in Actions.Where(tuple => tuple.condition(entity)).Select(tuple => tuple.action))
            action(entity);

        if (temp) EntityRegistry.AddTemp(entity);
        else EntityRegistry.Add(entity);

        return entity;
    }
}