using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[SlashCommandGroup("statistics", "Some sort of statistics")]
public class StatisticsModule(
    GameServer gameServer
) : ApplicationCommandModule {
    [SlashCommand("count", "Get current players and battles count"), SlashCooldown(1, 60, SlashCooldownBucketType.Channel)]
    public async Task Count(InteractionContext ctx) {
        await ctx.DeferAsync();

        int players = gameServer.PlayerConnections.Count;
        int battles = gameServer.BattleProcessor.BattlesCount;

        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed("")
            .AddField("Players", $"{players}", true)
            .AddField("Battles", $"{battles}", true);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}