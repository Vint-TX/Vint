using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Templates.Notification;

public class LeagueFirstEntranceRewardPersistentNotificationTemplate : NotificationTemplate {
    public IEntity Create(Dictionary<IEntity, int> rewards) {
        IEntity entity = Create("notification/leaguefirstentrancereward");

        entity.AddComponent(new LeagueFirstEntranceRewardNotificationComponent(rewards));
        return entity;
    }
}