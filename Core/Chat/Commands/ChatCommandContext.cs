using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Chat.Commands.Attributes;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Chat.Commands;

public sealed class ChatCommandContext(
    IServiceProvider serviceProvider,
    IChatCommandProcessor chatCommandProcessor,
    IPlayerConnection connection,
    IEntity chat,
    ChatCommandAttribute commandInfo
) {
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public IChatCommandProcessor ChatCommandProcessor { get; } = chatCommandProcessor;
    public IPlayerConnection Connection { get; } = connection;
    public IEntity Chat { get; } = chat;
    public ChatCommandAttribute CommandInfo { get; } = commandInfo;

    public async Task SendPrivateResponse(string response) =>
        await SendResponse(response, Chat, [Connection]);

    public async Task SendPublicResponse(string response) =>
        await SendResponse(response, Chat, ChatUtils.GetReceivers(ServiceProvider.GetRequiredService<GameServer>(), Connection, Chat));

    public async Task SendResponse(string response, IEntity chat, IAsyncEnumerable<IPlayerConnection> receivers) =>
        await ChatUtils.SendMessage(response, chat, receivers, null);

    public async Task SendResponse(string response, IEntity chat, IEnumerable<IPlayerConnection> receivers) =>
        await ChatUtils.SendMessage(response, chat, receivers, null);
}
