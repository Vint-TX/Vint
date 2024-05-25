using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1523947810296)]
public class LoginRewardNotificationTemplate : NotificationTemplate {
    public IEntity Create(List<LoginRewardItem> currentRewards, List<LoginRewardItem> allRewards, int currentDay) {
        IEntity entity = Create("notification/loginrewards");

        entity.AddComponent(new LoginRewardsNotificationComponent(currentRewards, allRewards, currentDay));
        return entity;
    }
}
