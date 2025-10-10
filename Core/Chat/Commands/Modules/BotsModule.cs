using Vint.Core.Battle.Autopilot;
using Vint.Core.Battle.Autopilot.Connection;
using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Properties;
using Vint.Core.Chat.Commands.Attributes;
using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.Utils;

namespace Vint.Core.Chat.Commands.Modules;

[ChatCommandGroup("bots", "Bot control commands", PlayerGroups.None)]
public class BotsModule(
    BotBuilder botBuilder
) : ChatCommandModule {
    [ChatCommand("addBot", "Add a bot to the round")]
    [RequireConditions(ChatCommandConditions.LobbyOwner)]
    public async Task AddBot(ChatCommandContext ctx) {
        LobbyBase lobby = ctx.Connection.LobbyPlayer!.Lobby;

        if (lobby.Players.Count >= lobby.Properties.GetValue(BattleProperty.MaxPlayers)) {
            await ctx.SendPrivateResponse("Lobby is full");
            return;
        }

        List<string> availableNicknames = ConfigManager.BotNicknames
            .Except(lobby.Players.Select(player => player.Connection.Player.Username))
            .ToList();

        string nickname = availableNicknames.RandomElement();

        BotConnection bot = await AddBot(nickname, lobby);
        await ctx.SendPrivateResponse($"Bot '{nickname}' added to the lobby");
        await lobby.PlayerReady(bot.LobbyPlayer!);
    }

    [ChatCommand("fillBots", "Fill the lobby with bots")]
    [RequireConditions(ChatCommandConditions.LobbyOwner)]
    public async Task FillBots(ChatCommandContext ctx, [Option("count", "Number of bots to add", true)] int count = -1) {
        LobbyBase lobby = ctx.Connection.LobbyPlayer!.Lobby;
        int maxPlayers = lobby.Properties.GetValue(BattleProperty.MaxPlayers);

        if (count == -1)
            count = maxPlayers - lobby.Players.Count;

        if (lobby.Players.Count >= maxPlayers) {
            await ctx.SendPrivateResponse($"Lobby is full for {count} bots");
            return;
        }

        int addedBots = 0;

        List<string> availableNicknames = ConfigManager.BotNicknames
            .Except(lobby.Players.Select(player => player.Connection.Player.Username))
            .ToList();

        for (; addedBots < count; addedBots++) {
            if (lobby.Players.Count >= maxPlayers) {
                await ctx.SendPrivateResponse("Lobby is full");
                break;
            }

            string nickname = availableNicknames.RandomElement();
            availableNicknames.Remove(nickname);

            BotConnection bot = await AddBot(nickname, lobby);
            await ctx.SendPrivateResponse($"Bot '{nickname}' added to the lobby");
            await lobby.PlayerReady(bot.LobbyPlayer!);
        }

        await ctx.SendPrivateResponse($"Added {addedBots} bots to the lobby");
    }

    async Task<BotConnection> AddBot(string nickname, LobbyBase lobby) {
        BotConnection bot = await botBuilder.ConnectNewBot(nickname);
        await bot.CalculateAndSetStatisticsByLobby(lobby);
        await lobby.AddPlayer(bot);
        return bot;
    }
}
