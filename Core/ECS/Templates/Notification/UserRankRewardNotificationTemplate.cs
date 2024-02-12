using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(636147223268818488)]
public class UserRankRewardNotificationTemplate : NotificationTemplate {
    public IEntity Create(long rank, long crystals, long xCrystals) {
        IEntity entity = Create("notification/rankreward");

        entity.AddComponent(new UserRankRewardNotificationInfoComponent(rank, crystals, xCrystals));
        return entity;
    }
}