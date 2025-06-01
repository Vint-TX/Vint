using System.Collections.Frozen;
using Vint.Core.Battle.Chat.Templates;
using Vint.Core.Battle.Player;
using Vint.Core.Chat.Components;
using Vint.Core.Chat.Events;
using Vint.Core.Chat.Templates;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Squads.Templates;
using Vint.Core.User.Components;

namespace Vint.Core.Chat;

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

    public static async Task SendMessage(string message, IEntity chat, IAsyncEnumerable<IPlayerConnection> receivers, IPlayerConnection? sender) {
        ChatMessageReceivedEvent messageEvent = CreateMessageEvent(message, sender);

        await foreach (IPlayerConnection receiver in receivers)
            await receiver.Send(messageEvent, chat);
    }

    public static async IAsyncEnumerable<IPlayerConnection> GetReceivers(GameServer server, IPlayerConnection from, IEntity chat) {
        switch (chat.TemplateAccessor?.Template) {
            case GeneralChatTemplate: {
                foreach (IPlayerConnection connection in server.PlayerConnections.Values.Where(conn => conn.IsLoggedIn))
                    yield return connection;

                yield break;
            }

            case BattleLobbyChatTemplate: {
                foreach (IPlayerConnection connection in from.LobbyPlayer!.Lobby.Players.Select(player => player.Connection))
                    yield return connection;

                yield break;
            }

            case GeneralBattleChatTemplate: {
                foreach (IPlayerConnection connection in from.LobbyPlayer!.Round!.Players.Select(player => player.Connection))
                    yield return connection;

                yield break;
            }

            case PersonalChatTemplate: {
                List<IEntity> users = [];

                foreach (IPlayerConnection connection in server
                             .FindConnections(chat.GetComponent<ChatParticipantsComponent>().Users.Select(user => user.Id).ToFrozenSet())) {
                    if (connection is { IsLoggedIn: true })
                        users.Add(connection.UserContainer.Entity);

                    await from.UserContainer.ShareTo(connection);
                    yield return connection;
                }

                await chat.ChangeComponent<ChatParticipantsComponent>(component => component.Users = users);
                yield break;
            }

            case TeamBattleChatTemplate: {
                foreach (IPlayerConnection connection in from.LobbyPlayer!.Round!.Tankers
                             .Where(tanker => tanker.TeamColor == from.LobbyPlayer.TeamColor)
                             .Select(tanker => tanker.Connection)) {
                    yield return connection;
                }

                yield break;
            }

            case SquadChatTemplate: {
                foreach (IPlayerConnection connection in from.Squad!.Members)
                    yield return connection;

                yield break;
            }

            default:
                yield break;
        }
    }

    public static IEntity GetChat(IPlayerConnection connection) {
        if (!connection.InLobby)
            return GlobalChat;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;

        return lobbyPlayer.InRound
            ? lobbyPlayer.Round.ChatEntity
            : lobbyPlayer.Lobby.ChatEntity;
    }
}
