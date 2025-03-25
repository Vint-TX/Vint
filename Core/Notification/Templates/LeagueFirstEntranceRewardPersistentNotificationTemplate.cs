using Vint.Core.ECS.Entities;
using Vint.Core.Notification.Components;

namespace Vint.Core.Notification.Templates;

public class LeagueFirstEntranceRewardPersistentNotificationTemplate : NotificationTemplate {
    public IEntity Create(Dictionary<IEntity, int> rewards) {
        IEntity entity = Create("notification/leaguefirstentrancereward");

        entity.AddComponent(new LeagueFirstEntranceRewardNotificationComponent(rewards));
        return entity;
    }
}
