using System.Text.RegularExpressions;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;

namespace Vint.Core.Utils;

public static class RegexUtils {
    public static EntranceValidationRulesComponent ValidationRules { get; } =
        ConfigManager.GetComponent<EntranceValidationRulesComponent>("lobby/entrance/authentication");

    static Regex EmailRegex { get; } = new(ValidationRules.EmailRegex);

    static Regex DiscordUsernameRegex { get; } = new(ValidationRules.DiscordUsernameRegex);

    static Regex LoginBeginingRegex { get; } = new(ValidationRules.LoginBeginingRegex);

    static Regex LoginEndingRegex { get; } = new(ValidationRules.LoginEndingRegex);

    static Regex LoginRegex { get; } = new(ValidationRules.LoginRegex);

    static Regex LoginSpecTogetherRegex { get; } = new(ValidationRules.LoginSpecTogetherRegex);

    static Regex LoginSymbolsRegex { get; } = new(ValidationRules.LoginSymbolsRegex);

    static Regex PasswordRegex { get; } = new(ValidationRules.PasswordRegex);

    public static bool IsEmailValid(string email) => EmailRegex.IsMatch(email);

    public static bool IsDiscordUsernameValid(string username) => DiscordUsernameRegex.IsMatch(username);

    static bool IsLoginSymbolsValid(string login) => LoginSymbolsRegex.IsMatch(login);

    static bool IsLoginBeginingValid(string login) => LoginBeginingRegex.IsMatch(login);

    static bool IsLoginEndingValid(string login) => LoginEndingRegex.IsMatch(login);

    static bool AreSpecSymbolsTogether(string login) => LoginSpecTogetherRegex.IsMatch(login);

    static bool IsPasswordSymbolsValid(string password) => PasswordRegex.IsMatch(password);

    static bool IsLoginTooShort(string login) => login.Length < ValidationRules.MinLoginLength;

    static bool IsLoginTooLong(string login) => login.Length > ValidationRules.MaxLoginLength;

    static bool IsPasswordTooShort(string password) => password.Length < ValidationRules.MinPasswordLength;

    static bool IsPasswordTooLong(string password) => password.Length > ValidationRules.MaxPasswordLength;

    public static bool IsLoginValid(string login) => !IsLoginTooShort(login) &&
                                                     !IsLoginTooLong(login) &&
                                                     IsLoginSymbolsValid(login) &&
                                                     IsLoginBeginingValid(login) &&
                                                     IsLoginEndingValid(login) &&
                                                     !AreSpecSymbolsTogether(login) &&
                                                     LoginRegex.IsMatch(login);
}
