using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Chat;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Server.Game;

namespace Vint.Core.Utils;

public static class ChatUtils {
    public static bool CensorshipEnabled => false;
    static IEntity GlobalChat => GlobalEntities.GetEntity("chats", "En");

    static ChatMessageReceivedEvent CreateMessageEvent(string message, IPlayerConnection? sender) {
        bool isSystem = sender == null;

        long userId = isSystem ? 0 : sender!.Player.Id;
        string avatarId = isSystem ? "" : sender!.UserContainer.Entity.GetComponent<UserAvatarComponent>().Id;
        string username = isSystem ? "System" : sender!.Player.Username;

        return new ChatMessageReceivedEvent(username, message, userId, avatarId, isSystem);
    }

    public static async Task SendMessage(string message, IEntity chat, IEnumerable<IPlayerConnection> receivers, IPlayerConnection? sender) =>
        await receivers.Send(CreateMessageEvent(message, sender), chat);

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
                    IPlayerConnection connection = server.PlayerConnections.Values
                        .Where(conn => conn.IsLoggedIn)
                        .SingleOrDefault(conn => conn.UserContainer.Id == user.Id)!;

                    from.UserContainer.ShareTo(connection).GetAwaiter().GetResult(); // todo fuck
                    return connection;
                })
                .Where(conn => conn != null!),

            TeamBattleChatTemplate => from.LobbyPlayer!.Round!.Tankers
                .Where(tanker => tanker.TeamColor == from.LobbyPlayer.TeamColor)
                .Select(tanker => tanker.Connection),

            SquadChatTemplate => from.Squad!.Members,

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
