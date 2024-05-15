using Vint.Core.ChatCommands.Attributes;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ChatCommands;

public sealed class ChatCommandContext(
    IChatCommandProcessor chatCommandProcessor,
    IPlayerConnection connection,
    IEntity chat,
    ChatCommandAttribute commandInfo
) {
    public IChatCommandProcessor ChatCommandProcessor { get; } = chatCommandProcessor;
    public IPlayerConnection Connection { get; } = connection;
    public IEntity Chat { get; } = chat;
    public ChatCommandAttribute CommandInfo { get; } = commandInfo;
    public GameServer GameServer => Connection.Server;

    public async Task SendPrivateResponse(string response) =>
        await SendResponse(response, Chat, [Connection]);

    public async Task SendPublicResponse(string response) =>
        await SendResponse(response, Chat, ChatUtils.GetReceivers(Connection, Chat));

    public async Task SendResponse(string response, IEntity chat, IEnumerable<IPlayerConnection> receivers) =>
        await ChatUtils.SendMessage(response, chat, receivers, null);

    public void DisplayMessage(string message) => Connection.DisplayMessage(message);
}
