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

    public void SendPrivateResponse(string response) =>
        SendResponse(response, Chat, [Connection]);

    public void SendPublicResponse(string response) =>
        SendResponse(response, Chat, ChatUtils.GetReceivers(Connection, Chat));

    public void SendResponse(string response, IEntity chat, IEnumerable<IPlayerConnection> receivers) =>
        ChatUtils.SendMessage(response, chat, receivers, null);
}