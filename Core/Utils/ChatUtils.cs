using LinqToDB;
using Vint.Core.Battle.Player;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Chat;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Server.Game;

namespace Vint.Core.Utils;

public static class ChatUtils {
    public static bool CensorshipEnabled => false;
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
        await using DbConnection db = new();

        bool isSystem = sender == null;
        bool isBlocked = !isSystem &&
                         await db.Relations.SingleOrDefaultAsync(relation => relation.SourcePlayerId == receiver.Player.Id &&
                                                                             relation.TargetPlayerId == sender!.Player.Id &&
                                                                             (relation.Types & RelationTypes.Blocked) == RelationTypes.Blocked) != null;

        if (isBlocked) return null;

        string receiverLocale = receiver.Player.CountryCode.ToUpper() switch {
            "RU" => "RU",
            "EN" => "EN",
            _ => "EN"
        };

        Dictionary<string, string> localizedStrings = Localization[receiverLocale];

        long userId = isSystem ? 0 : sender!.Player.Id;
        string avatarId = isSystem ? "" : sender!.UserContainer.Entity.GetComponent<UserAvatarComponent>().Id;
        string username = isSystem
            ? localizedStrings["SystemUsername"]
            : isBlocked
                ? localizedStrings["BlockedUsername"]
                : sender!.Player.Username;

        message = isBlocked
            ? localizedStrings["BlockedMessage"]
            : message;

        return new ChatMessageReceivedEvent(username, message, userId, avatarId, isSystem);
    }

    public static async Task SendMessage(string message, IEntity chat, IEnumerable<IPlayerConnection> receivers, IPlayerConnection? sender) {
        foreach (IPlayerConnection receiver in receivers) {
            ChatMessageReceivedEvent? messageEvent = await CreateMessageEvent(message, receiver, sender);

            if (messageEvent == null) continue;

            await receiver.Send(messageEvent, chat);
        }
    }

    // todo REWRITE, squads
    public static IEnumerable<IPlayerConnection> GetReceivers(GameServer server, IPlayerConnection from, IEntity chat) =>
        chat.TemplateAccessor?.Template switch {
            GeneralChatTemplate => server.PlayerConnections.Values.Where(conn => conn.IsLoggedIn),

            BattleLobbyChatTemplate => from.LobbyPlayer!.Lobby.Players.Select(player => player.Connection),

            GeneralBattleChatTemplate => from.LobbyPlayer!.Round!.Players.Select(player => player.Connection),

            PersonalChatTemplate => chat
                .GetComponent<ChatParticipantsComponent>().Users
                .ToList()
                .Select(user => {
                    IPlayerConnection connection = server
                        .PlayerConnections
                        .Values
                        .Where(conn => conn.IsLoggedIn)
                        .SingleOrDefault(conn => conn.UserContainer.Id == user.Id)!;

                    from.UserContainer.ShareTo(connection).GetAwaiter().GetResult(); // todo fuck
                    return connection;
                })
                .Where(conn => conn != null!),

            TeamBattleChatTemplate => from.LobbyPlayer!.Round!.Tankers
                .Where(tanker => tanker.TeamColor == from.LobbyPlayer.TeamColor)
                .Select(tanker => tanker.Connection),

            _ => []
        };

    public static IEntity GetChat(IPlayerConnection connection) {
        if (!connection.InLobby)
            return GlobalChat;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;

        return lobbyPlayer.InRound
            ? lobbyPlayer.Round.ChatEntity
            : lobbyPlayer.Lobby.ChatEntity;
    }
}
