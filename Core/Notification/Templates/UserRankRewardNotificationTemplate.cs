using Vint.Core.ECS.Entities;
using Vint.Core.Notification.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Notification.Templates;

[ProtocolId(636147223268818488)]
public class UserRankRewardNotificationTemplate : NotificationTemplate {
    public IEntity Create(long rank, long crystals, long xCrystals) {
        IEntity entity = Create("notification/rankreward");

        entity.AddComponent(new UserRankRewardNotificationInfoComponent(rank, crystals, xCrystals));
        return entity;
    }
}
