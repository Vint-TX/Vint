using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Item;
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
        connection.Share(connection.GetEntities());
        connection.User.AddComponent(new UserAvatarComponent());

        Player player = connection.Player;
        Preset preset = player.Presets[player.CurrentPresetIndex];

        foreach (IEntity entity in new[] {
                     connection.GetEntity(player.CurrentAvatarId)!.GetUserEntity(connection),
                     preset.Hull.GetUserEntity(connection),
                     preset.Paint.GetUserEntity(connection),
                     preset.HullSkin.GetUserEntity(connection),
                     preset.Weapon.GetUserEntity(connection),
                     preset.Cover.GetUserEntity(connection),
                     preset.WeaponSkin.GetUserEntity(connection),
                     preset.Shell.GetUserEntity(connection),
                     preset.Graffiti.GetUserEntity(connection)
                 }) {
            entity.AddComponent(new MountedItemComponent());
        }

        connection.ClientSession.Send(new PaymentSectionLoadedEvent());

        connection.ClientSession.Send(
            new FriendsLoadedEvent(
                connection.Player.AcceptedFriendIds.ToHashSet(),
                connection.Player.IncomingFriendIds.ToHashSet(),
                connection.Player.OutgoingFriendIds.ToHashSet()));
    }
}
