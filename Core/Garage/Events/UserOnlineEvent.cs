using Vint.Core.Battle.Modules.Common.Components;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Shop.Events;
using Vint.Core.User.Events.Relationship;

namespace Vint.Core.Garage.Events;

[ProtocolId(1507022246767)]
public class UserOnlineEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await connection.Share(connection.GetEntities());

        Player player = connection.Player;
        Preset preset = player.CurrentPreset;

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
                     }.Distinct()) {
            await entity.AddComponent<MountedItemComponent>();
        }

        foreach (PresetModule presetModule in preset.Modules) {
            IEntity module = presetModule.Entity.GetUserModule(connection);
            IEntity slot = presetModule.GetSlotEntity(connection);

            await module.AddComponent<MountedItemComponent>();
            await slot.AddComponentFrom<ModuleGroupComponent>(module);
        }

        await connection.Send<PaymentSectionLoadedEvent>();

        await using DbConnection db = new();

        HashSet<long> friendIds = db.Friends
            .Where(friend => friend.UserId == player.Id)
            .Select(friend => friend.FriendId)
            .ToHashSet();

        HashSet<long> incomingIds = db.FriendRequests
            .Where(request => request.FriendId == player.Id)
            .Select(request => request.SenderId)
            .ToHashSet();

        HashSet<long> outgoingIds = db.FriendRequests
            .Where(request => request.SenderId == player.Id)
            .Select(request => request.FriendId)
            .ToHashSet();

        await connection.Send(new FriendsLoadedEvent(friendIds, incomingIds, outgoingIds));
    }
}
