using System.Diagnostics.CodeAnalysis;
using Vint.Core.ChatCommands;
using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Chat;
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

    public void Execute(IPlayerConnection sender, IEnumerable<IEntity> entities) {
        IEntity chat = entities.Single();

        if (ChatCommandProcessor.TryParseCommand(Message, out ChatCommand? chatCommand)) {
            if (chatCommand == null)
                ChatUtils.SendMessage("Unknown command", chat, [sender], null);
            else 
                ChatCommandProcessor.Execute(sender, chat, Message, chatCommand);
            
            return;
        }
        
        Punishment? mute = sender.Player.GetMuteInfo();

        if (mute is { Active: true }) {
            ChatUtils.SendMessage($"You have been {mute}", chat, [sender], null);
            return;
        }
        
        TemplateAccessor chatTemplateAccessor = chat.TemplateAccessor!;
        Message = Message.Trim();

        if (!Validate(chatTemplateAccessor.ConfigPath!)) return;

        IEnumerable<IPlayerConnection> receivers = chatTemplateAccessor.Template switch { // todo
            GeneralChatTemplate => sender.Server.PlayerConnections,

            BattleLobbyChatTemplate => sender.BattlePlayer!.Battle.Players
                .Select(battlePlayer => battlePlayer.PlayerConnection),

            GeneralBattleChatTemplate => sender.BattlePlayer!.Battle.Players
                .Where(battlePlayer => battlePlayer.InBattle)
                .Select(battlePlayer => battlePlayer.PlayerConnection),

            PersonalChatTemplate => chat.GetComponent<ChatParticipantsComponent>().Users
                .Select(user => {
                    IPlayerConnection? connection = sender.Server.PlayerConnections
                        .Where(conn => conn.IsOnline)
                        .SingleOrDefault(conn => conn.User.Id == user.Id);

                    connection?.ShareIfUnshared(chat, sender.User);
                    return connection!;
                })
                .Where(conn => conn != null!),

            _ => []
        };

        ChatUtils.SendMessage(Message, chat, receivers, sender);
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