using Serilog.Templates.Themes;

namespace Vint.Core.Logging;

public class CustomTemplateTheme : TemplateTheme {
    public CustomTemplateTheme() : base(AnsiStyles) { }
    public CustomTemplateTheme(TemplateTheme baseTheme) : base(baseTheme, AnsiStyles) { }

    static Dictionary<TemplateThemeStyle, string> AnsiStyles { get; } = new() {
        [TemplateThemeStyle.Text] = "\e[38;5;0253m",
        [TemplateThemeStyle.SecondaryText] = "\e[38;5;0246m",
        [TemplateThemeStyle.TertiaryText] = "\e[38;5;0242m",
        [TemplateThemeStyle.Invalid] = "\e[33;1m",
        [TemplateThemeStyle.Null] = "\e[38;5;0038m",
        [TemplateThemeStyle.Name] = "\e[38;5;0081m",
        [TemplateThemeStyle.String] = "\e[38;5;0216m",
        [TemplateThemeStyle.Number] = "\e[38;5;151m",
        [TemplateThemeStyle.Boolean] = "\e[38;5;0038m",
        [TemplateThemeStyle.Scalar] = "\e[38;5;0079m",
        [TemplateThemeStyle.LevelVerbose] = "\e[34m",
        [TemplateThemeStyle.LevelDebug] = "\e[36m",
        [TemplateThemeStyle.LevelInformation] = "\e[32m",
        [TemplateThemeStyle.LevelWarning] = "\e[33;1m",
        [TemplateThemeStyle.LevelError] = "\e[31;1m",
        [TemplateThemeStyle.LevelFatal] = "\e[31;1m"
    };
}
