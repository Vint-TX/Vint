using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Chat;

[ProtocolId(636469080057216111)]
public class CreatePrivateChatEvent : IServerEvent {
    [ProtocolName("UserUid")] public string Username { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.Player.Username == Username) return Task.CompletedTask;

        IPlayerConnection? targetConnection = connection.Server.PlayerConnections.Values
            .Where(playerConnection => playerConnection.IsOnline)
            .SingleOrDefault(playerConnection => playerConnection.Player.Username == Username);

        if (targetConnection == null) {
            connection.DisplayMessage($"{Username} is offline");
            return Task.CompletedTask;
        }

        IEntity? chat = connection.User.GetComponent<PersonalChatOwnerComponent>().Chats
            .Concat(targetConnection.User.GetComponent<PersonalChatOwnerComponent>().Chats)
            .Select(chat => new { Chat = chat, chat.GetComponent<ChatParticipantsComponent>().Users })
            .FirstOrDefault(x => x.Users.Contains(connection.User) &&
                                 x.Users.Contains(targetConnection.User))?.Chat;

        if (chat == null) {
            chat = new PersonalChatTemplate().Create(connection.User, targetConnection.User);
            connection.ShareIfUnshared(targetConnection.User);
        } else {
            connection.User.ChangeComponent<PersonalChatOwnerComponent>(component => component.Chats.Remove(chat));
            connection.Unshare(chat);
        }

        connection.User.ChangeComponent<PersonalChatOwnerComponent>(component => component.Chats.Add(chat));
        connection.Share(chat);
        return Task.CompletedTask;
    }
}
