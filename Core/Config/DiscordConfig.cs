namespace Vint.Core.Config;

public readonly record struct DiscordConfig(
    string Oauth2Url,
    ulong GuildId,
    ulong ReportsChannelId
);
