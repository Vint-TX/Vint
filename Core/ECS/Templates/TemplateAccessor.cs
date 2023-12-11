namespace Vint.Core.ECS.Templates;

public readonly record struct TemplateAccessor(
    EntityTemplate Template,
    string? ConfigPath
) {
    public override string ToString() => $"TemplateAccessor {{ Template: {Template.GetType().Name}, ConfigPath: '{ConfigPath ?? "null"}' }}";
}
