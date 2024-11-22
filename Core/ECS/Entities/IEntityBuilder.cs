using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Templates;

namespace Vint.Core.ECS.Entities;

public interface IEntityBuilder {
    long Id { get; }
    TemplateAccessor? TemplateAccessor { get; }
    List<IComponent> Components { get; }

    IEntityBuilder WithTemplateAccessor(TemplateAccessor templateAccessor);

    IEntityBuilder WithTemplateAccessor(EntityTemplate template, string? configPath);

    IEntityBuilder WithId(long id);

    IEntityBuilder AddComponent(IComponent component);

    IEntityBuilder AddComponent<T>() where T : class, IComponent, new();

    IEntityBuilder AddComponent<T>(string configPath) where T : class, IComponent;

    IEntityBuilder AddGroupComponent<T>(IEntity? key = null) where T : GroupComponent;

    IEntityBuilder AddComponentFrom<T>(IEntity entity) where T : class, IComponent;

    IEntityBuilder AddComponentFromConfig<T>() where T : class, IComponent;

    IEntityBuilder TryAddComponent<T>(IEntity entity) where T : class, IComponent;

    IEntityBuilder TryAddComponent<T>(string configPath) where T : class, IComponent;

    IEntityBuilder ThenExecuteIf(Func<IEntity, bool> condition, Action<IEntity> action);

    IEntity Build(bool temp);
}
