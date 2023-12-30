using System.Diagnostics.CodeAnalysis;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Chat;

[ProtocolId(1446035600297)]
public class SendChatMessageEvent : IServerEvent {
    static Dictionary<string, ChatConfigComponent> ConfigPathToConfig { get; } = new();
    public string Message { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity chat = entities.Single();
        TemplateAccessor chatTemplateAccessor = chat.TemplateAccessor!;
        Message = Message.Trim();

        if (!Validate(chatTemplateAccessor.ConfigPath!)) return;

        switch (chatTemplateAccessor.Template) { // todo
            case GeneralChatTemplate: {
                ChatUtils.SendMessage(Message, chat, connection.Server.PlayerConnections, connection);
                break;
            }
        }
    }

    bool Validate(string chatConfigPath) { // todo
        ChatConfigComponent chatConfig = GetChatConfig(chatConfigPath);

        return Message.Length > 0 && Message.Length <= chatConfig.MaxMessageLength;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    static ChatConfigComponent GetChatConfig(string path) {
        if (!ConfigPathToConfig.TryGetValue(path, out ChatConfigComponent? chatConfig)) {
            chatConfig = ConfigManager.GetComponent<ChatConfigComponent>(path);
            ConfigPathToConfig[path] = chatConfig;
        }

        return chatConfig;
    }
}