using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Payment;
using Vint.Core.ECS.Events.User.Friends;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Entrance.Lobby;

[ProtocolId(1507022246767)]
public class UserOnlineEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        //todo

        connection.User.AddComponent(new UserAvatarComponent());

        //todo

        connection.ClientSession.Send(new PaymentSectionLoadedEvent());
        connection.ClientSession.Send(
            new FriendsLoadedEvent(
                connection.Player.AcceptedFriendIds,
                connection.Player.IncomingFriendIds,
                connection.Player.OutgoingFriendIds));
    }
}
