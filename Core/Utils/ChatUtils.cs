using LinqToDB;
using Vint.Core.Battles;
using Vint.Core.Battles.Player;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Chat;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Server;

namespace Vint.Core.Utils;

public static class ChatUtils {
    public static bool CensorshipEnabled => true;
    public static IEntity GlobalChat => GlobalEntities.GetEntity("chats", "En");

    public static Dictionary<string, Dictionary<string, string>> Localization { get; } = new() { // hardcoded, todo parse from configs
        {
            "RU", new Dictionary<string, string> {
                { "SystemUsername", "Системное сообщение" },
                { "BlockedUsername", "Заблокированный игрок" },
                { "BlockedMessage", "[Заблокировано]" }
            }
        }, {
            "EN", new Dictionary<string, string> {
                { "SystemUsername", "System message" },
                { "BlockedUsername", "Blocked player" },
                { "BlockedMessage", "[Blocked]" }
            }
        }
    };

    public static async Task<ChatMessageReceivedEvent?> CreateMessageEvent(string message, IPlayerConnection receiver, IPlayerConnection? sender) {
        bool isSystem = sender == null;

        await using DbConnection db = new();

        bool isBlocked = !isSystem &&
                         await db.Relations.SingleOrDefaultAsync(relation => relation.SourcePlayerId == receiver.Player.Id &&
                                                                             relation.TargetPlayerId == sender!.Player.Id &&
                                                                             (relation.Types & RelationTypes.Blocked) == RelationTypes.Blocked) !=
                         null;

        if (isBlocked) return null;

        string receiverLocale = receiver.Player.CountryCode.ToUpper() switch {
            "RU" => "RU",
            "EN" => "EN",
            _ => "EN"
        };

        Dictionary<string, string> localizedStrings = Localization[receiverLocale];

        long userId = isSystem ? 0 : sender!.Player.Id;
        string avatarId = isSystem ? "" : sender!.User.GetComponent<UserAvatarComponent>().Id;

        string username = isSystem ? localizedStrings["SystemUsername"]
                          : isBlocked ? localizedStrings["BlockedUsername"]
                          : sender!.Player.Username;

        message = isBlocked ? localizedStrings["BlockedMessage"] : message;

        return new ChatMessageReceivedEvent(username, message, userId, avatarId, isSystem);
    }

    public static async Task SendMessage(string message, IEntity chat, IEnumerable<IPlayerConnection> receivers, IPlayerConnection? sender) {
        foreach (IPlayerConnection receiver in receivers) {
            ChatMessageReceivedEvent? messageEvent = await CreateMessageEvent(message, receiver, sender);

            if (messageEvent == null) continue;

            await receiver.Send(messageEvent, chat);
        }
    }

    // todo squads
    public static IEnumerable<IPlayerConnection> GetReceivers(IPlayerConnection from, IEntity chat) => chat.TemplateAccessor?.Template switch {
        GeneralChatTemplate => from.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline),

        BattleLobbyChatTemplate => from.BattlePlayer!.Battle.Players
            .Select(battlePlayer => battlePlayer.PlayerConnection),

        GeneralBattleChatTemplate => from.BattlePlayer!.Battle.Players
            .Where(battlePlayer => battlePlayer.InBattle)
            .Select(battlePlayer => battlePlayer.PlayerConnection),

        PersonalChatTemplate => chat.GetComponent<ChatParticipantsComponent>().Users
            .ToList()
            .Select(user => {
                IPlayerConnection? connection = from.Server.PlayerConnections.Values
                    .Where(conn => conn.IsOnline)
                    .SingleOrDefault(conn => conn.User.Id == user.Id);

                connection?.ShareIfUnshared(chat, from.User);
                return connection!;
            })
            .Where(conn => conn != null!),

        TeamBattleChatTemplate => from.BattlePlayer!.Battle.Players
            .Where(battlePlayer => battlePlayer.TeamColor == from.BattlePlayer.TeamColor)
            .Select(battlePlayer => battlePlayer.PlayerConnection),

        _ => []
    };

    public static IEntity GetChat(IPlayerConnection connection) {
        if (!connection.InLobby)
            return GlobalChat;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battle battle = battlePlayer.Battle;

        return battlePlayer.InBattle ? battle.BattleChatEntity : battle.LobbyChatEntity;
    }
}
