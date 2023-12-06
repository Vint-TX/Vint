namespace Vint.Core.ECS.Templates;

public readonly record struct TemplateAccessor(
    IEntityTemplate Template,
    string? ConfigPath
);