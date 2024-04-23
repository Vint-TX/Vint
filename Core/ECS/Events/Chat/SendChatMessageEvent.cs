using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Vint.Core.ChatCommands;
using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;
using YamlDotNet.Core;

namespace Vint.Core.ECS.Events.Chat;

[ProtocolId(1446035600297)]
public class SendChatMessageEvent : IServerEvent {
    static Dictionary<string, ChatConfigComponent> ConfigPathToConfig { get; } = new();

    public string Message { get; private set; } = null!;

    public void Execute(IPlayerConnection sender, IEnumerable<IEntity> entities) {
        IEntity chat = entities.Single();

        if (sender.Server.ChatCommandProcessor.TryParseCommand(Message, out ChatCommand? chatCommand)) {
            if (chatCommand == null)
                ChatUtils.SendMessage("Unknown command", chat, [sender], null);
            else
                chatCommand.Execute(sender, chat, Message);

            return;
        }

        Punishment? mute = sender.Player.GetMuteInfo();

        if (mute is { Active: true }) {
            ChatUtils.SendMessage($"You have been {mute}", chat, [sender], null);
            return;
        }

        Message = Message.Trim();
        string checkingMessage = Message;
        if (!Validate(chat.TemplateAccessor!.ConfigPath!, ref checkingMessage)) {
            sender.Logger.ForType(GetType()).Warning($"Message failed validation: '{Message}', using {checkingMessage}");
        }
        Message = checkingMessage;
        ChatUtils.SendMessage(Message, chat, ChatUtils.GetReceivers(sender, chat), sender);
    }

    // todo ban-words (C6OI: I dont want to implement it, but I dont mind if someone sends a PR with it)
    static bool Validate(string chatConfigPath, ref string message) {
        ChatConfigComponent chatConfig = GetChatConfig(chatConfigPath);
        bool isCleanMessage = true;

        if (ConfigManager.BadWordsPatterns is not null) {
            foreach (var entry in ConfigManager.BadWordsPatterns) {
                string forbiddenWord = entry.Key;
                string forbiddenWordPattern = entry.Value;

                // Use Regex to detect the word with mixed scripts
                if (Regex.IsMatch(message, forbiddenWordPattern, RegexOptions.IgnoreCase)) {
                    isCleanMessage = false;
                    string maskedWord = new string('*', forbiddenWord.Length);
                    message = Regex.Replace(message, forbiddenWordPattern, maskedWord, RegexOptions.IgnoreCase);
                }
            }
        }
        return message.Length > 0 && message.Length <= chatConfig.MaxMessageLength && isCleanMessage;
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