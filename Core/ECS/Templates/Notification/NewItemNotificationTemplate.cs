using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1481176055388)]
public class NewItemNotificationTemplate : NotificationTemplate {
    public IEntity CreateRegular(IEntity user, IEntity item, int amount) {
        IEntity entity = Create("notification/newitem", user);

        entity.AddComponent(new NewItemNotificationComponent(item, amount));
        return entity;
    }

    public IEntity CreateCard(IEntity chest, IEntity item, int amount) {
        IEntity entity = CreateRegular(chest, item, amount);

        entity.AddComponent<NewCardItemNotificationComponent>();
        return entity;
    }
}
