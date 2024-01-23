using Vint.Core.ChatCommands.Attributes;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ChatCommands;

public sealed class ChatCommandContext(
    IPlayerConnection connection,
    IEntity chat,
    ChatCommandAttribute commandInfo
) {
    public IPlayerConnection Connection { get; } = connection;
    public IEntity Chat { get; } = chat;
    public ChatCommandAttribute CommandInfo { get; } = commandInfo;

    public void SendResponse(string response) => 
        ChatUtils.SendMessage(response, Chat, [Connection], null);
}