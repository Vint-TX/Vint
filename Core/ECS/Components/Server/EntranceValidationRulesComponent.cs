namespace Vint.Core.ECS.Components.Server;

public class EntranceValidationRulesComponent : IComponent {
    public string LoginRegex { get; private set; } = null!;
    public string LoginSymbolsRegex { get; private set; } = null!;
    public string LoginBeginingRegex { get; private set; } = null!;
    public string LoginEndingRegex { get; private set; } = null!;
    public string LoginSpecTogetherRegex { get; private set; } = null!;
    public string EmailRegex { get; private set; } = null!;
    public string DiscordUsernameRegex { get; private set; } = null!;
    public string PasswordRegex { get; private set; } = null!;
    public int MinLoginLength { get; private set; }
    public int MaxLoginLength { get; private set; }
    public int MinPasswordLength { get; private set; }
    public int MaxPasswordLength { get; private set; }
    public int MinEmailLength { get; private set; }
    public int MaxEmailLength { get; private set; }
    public int MinDiscordUsernameLength { get; private set; }
    public int MaxDiscordUsernameLength { get; private set; }
    public int MaxCaptchaLength { get; private set; }
}
