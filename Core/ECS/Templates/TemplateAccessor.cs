namespace Vint.Core.ECS.Templates;

public class TemplateAccessor(
    EntityTemplate template,
    string? configPath
) {
    public EntityTemplate Template { get; set; } = template;
    public string? ConfigPath { get; set; } = configPath;

    public override string ToString() =>
        $"TemplateAccessor {{ Template: {Template.GetType().Name}, ConfigPath: '{ConfigPath ?? "null"}' }}";
}