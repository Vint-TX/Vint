using Vint.Core.ECS.Entities;

namespace Vint.Core.Config;

public readonly record struct DiscordConfig(
    ulong GuildId,
    ulong ReportsChannelId,
    string OAuth2Redirect,
    string[] OAuth2Scopes,
    ulong LinkedRoleId,
    List<LinkReward> LinkRewards
);

public record struct LinkReward(
    string MarketEntityType,
    string MarketEntityName,
    int Amount
) {
    public IEntity MarketEntity => GlobalEntities.GetEntity(MarketEntityType, MarketEntityName);
}
