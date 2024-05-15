namespace Vint.Core.Discord;

public readonly record struct DiscordLinkRequest(
    string State,
    long UserId
);
