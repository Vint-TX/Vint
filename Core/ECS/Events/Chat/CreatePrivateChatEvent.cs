using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Chat;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Chat;

[ProtocolId(636469080057216111)]
public class CreatePrivateChatEvent(
    GameServer server
) : IServerEvent {
    [ProtocolName("UserUid")] public string Username { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.Player.Username == Username)
            return;

        IPlayerConnection? targetConnection = server
            .PlayerConnections
            .Values
            .Where(playerConnection => playerConnection.IsOnline)
            .SingleOrDefault(playerConnection => playerConnection.Player.Username == Username);

        if (targetConnection == null) {
            await connection.DisplayMessage($"{Username} is offline");
            return;
        }

        IEntity? chat = connection.UserContainer.Entity
            .GetComponent<PersonalChatOwnerComponent>().Chats
            .Concat(targetConnection.UserContainer.Entity.GetComponent<PersonalChatOwnerComponent>().Chats)
            .Select(chat => new {
                Chat = chat, chat.GetComponent<ChatParticipantsComponent>().Users
            })
            .FirstOrDefault(x => x.Users.Contains(connection.UserContainer.Entity) && x.Users.Contains(targetConnection.UserContainer.Entity))
            ?.Chat;

        if (chat == null) {
            chat = new PersonalChatTemplate().Create(connection.UserContainer.Entity, targetConnection.UserContainer.Entity);
            await connection.ShareIfUnshared(targetConnection.UserContainer.Entity);
        } else {
            await connection.UserContainer.Entity.ChangeComponent<PersonalChatOwnerComponent>(component => component.Chats.Remove(chat));
            await connection.Unshare(chat);
        }

        await connection.UserContainer.Entity.ChangeComponent<PersonalChatOwnerComponent>(component => component.Chats.Add(chat));
        await connection.Share(chat);
    }
}
