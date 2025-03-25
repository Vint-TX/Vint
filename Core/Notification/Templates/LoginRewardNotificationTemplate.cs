using Vint.Core.ECS.Entities;
using Vint.Core.Notification.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Notification.Templates;

[ProtocolId(1523947810296)]
public class LoginRewardNotificationTemplate : NotificationTemplate {
    public IEntity Create(List<LoginRewardItem> currentRewards, List<LoginRewardItem> allRewards, int currentDay) {
        IEntity entity = Create("notification/loginrewards");

        entity.AddComponent(new LoginRewardsNotificationComponent(currentRewards, allRewards, currentDay));
        return entity;
    }
}
