using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Payment;
using Vint.Core.ECS.Events.User.Friends;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Lobby;

[ProtocolId(1507022246767)]
public class UserOnlineEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.Share(connection.GetEntities());

        using DbConnection db = new();

        Player player = connection.Player;
        Preset preset = db.Presets.Single(preset => preset.PlayerId == player.Id && preset.Index == player.CurrentPresetIndex);

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

        connection.User.AddComponent(new UserAvatarComponent(connection, player.CurrentAvatarId));
        connection.Send(new PaymentSectionLoadedEvent());

        IQueryable<Relation> relations = db.Relations.Where(relation => relation.SourcePlayerId == player.Id);

        HashSet<long> friendIds = relations
            .Where(relation => (relation.Types & RelationTypes.Friend) == RelationTypes.Friend)
            .Select(relation => relation.TargetPlayerId)
            .ToHashSet();

        HashSet<long> incomingIds = relations
            .Where(relation => (relation.Types & RelationTypes.IncomingRequest) == RelationTypes.IncomingRequest)
            .Select(relation => relation.TargetPlayerId)
            .ToHashSet();

        HashSet<long> outgoingIds = relations
            .Where(relation => (relation.Types & RelationTypes.OutgoingRequest) == RelationTypes.OutgoingRequest)
            .Select(relation => relation.TargetPlayerId)
            .ToHashSet();

        connection.ClientSession.Send(new FriendsLoadedEvent(friendIds, incomingIds, outgoingIds));
    }
}