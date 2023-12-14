using Vint.Core.ECS.Components;
using Vint.Core.ECS.Templates;

namespace Vint.Core.ECS.Entities;

public interface IEntityBuilder {
    public long Id { get; }
    public TemplateAccessor? TemplateAccessor { get; }
    public HashSet<IComponent> Components { get; }

    public IEntityBuilder WithTemplateAccessor(TemplateAccessor templateAccessor);

    public IEntityBuilder WithTemplateAccessor(EntityTemplate template, string? configPath);

    public IEntityBuilder AddComponent(IComponent component);

    public IEntity Build();
}