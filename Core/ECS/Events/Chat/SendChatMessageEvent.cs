using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.ChatCommands;
using Vint.Core.Config;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Server.Chat;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Chat;

[ProtocolId(1446035600297)]
public class SendChatMessageEvent : IServerEvent {
    static Dictionary<string, ChatConfigComponent> ConfigPathToConfig { get; } = new();

    public string Message { get; private set; } = null!;

    public async Task Execute(IPlayerConnection sender, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        GameServer server = serviceProvider.GetRequiredService<GameServer>();
        IChatCommandProcessor chatCommandProcessor = serviceProvider.GetRequiredService<IChatCommandProcessor>();
        IEntity chat = entities.Single();

        if (chatCommandProcessor.TryParseCommand(Message, out ChatCommand? chatCommand)) {
            if (chatCommand == null)
                await ChatUtils.SendMessage("Unknown command", chat, [sender], null);
            else
                await chatCommand.Execute(sender, serviceProvider, chat, Message);

            return;
        }

        Punishment? mute = await sender.Player.GetMuteInfo();

        if (mute is { Active: true }) {
            await ChatUtils.SendMessage($"You have been {mute}", chat, [sender], null);
            return;
        }

        if (!Validate(chat.TemplateAccessor!.ConfigPath!)) {
            sender
                .Logger
                .ForType(GetType())
                .Warning("Message failed validation: '{Message}'", Message);

            return;
        }

        CleanupMessage();
        await ChatUtils.SendMessage(Message, chat, ChatUtils.GetReceivers(server, sender, chat), sender);
    }

    bool Validate(string chatConfigPath) {
        ChatConfigComponent chatConfig = GetChatConfig(chatConfigPath);

        return !string.IsNullOrWhiteSpace(Message) && Message.Length > 0 && Message.Length <= chatConfig.MaxMessageLength;
    }

    void CleanupMessage() {
        Message = Message.Trim();

        if (!ChatUtils.CensorshipEnabled) return;

        ConcurrentDictionary<string, Regex> badWords = new();

        Parallel.ForEach(ConfigManager.CensorshipRegexes,
            pair => {
                (string word, Regex regex) = pair;

                if (regex.IsMatch(Message))
                    badWords.TryAdd(word, regex);
            });

        foreach ((string word, Regex regex) in badWords) {
            foreach (Match match in regex.Matches(word))
                Message = regex.Replace(Message, new string('*', match.Value.Length));
        }
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
