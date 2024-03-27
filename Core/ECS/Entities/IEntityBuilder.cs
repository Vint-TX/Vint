using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Templates;

namespace Vint.Core.ECS.Entities;

public interface IEntityBuilder {
    public long Id { get; }
    public TemplateAccessor? TemplateAccessor { get; }
    public HashSet<IComponent> Components { get; }

    public IEntityBuilder WithTemplateAccessor(TemplateAccessor templateAccessor);

    public IEntityBuilder WithTemplateAccessor(EntityTemplate template, string? configPath);

    public IEntityBuilder WithId(long id);

    public IEntityBuilder AddComponent(IComponent component);

    public IEntityBuilder AddComponent<T>() where T : class, IComponent, new();

    public IEntityBuilder AddComponent<T>(string configPath) where T : class, IComponent;

    public IEntityBuilder AddGroupComponent<T>(IEntity? entity = null) where T : GroupComponent;

    public IEntityBuilder AddComponentFrom<T>(IEntity entity) where T : class, IComponent;

    public IEntityBuilder TryAddComponent<T>(IEntity entity) where T : class, IComponent;

    public IEntityBuilder TryAddComponent<T>(string configPath) where T : class, IComponent;

    public IEntityBuilder ThenExecuteIf(Func<IEntity, bool> condition, Action<IEntity> action);

    public IEntity Build(bool temp);
}