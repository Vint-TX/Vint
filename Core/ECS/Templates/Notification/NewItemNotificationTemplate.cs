using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1481176055388)]
public class NewItemNotificationTemplate : EntityTemplate {
    public IEntity CreateRegular(IEntity user, IEntity item, int amount) => Entity("notification/newitem",
        builder => builder
            .AddComponent(new NewItemNotificationComponent(item, amount))
            .AddComponent(new NotificationComponent(NotificationPriority.Message))
            .AddGroupComponent<NotificationGroupComponent>(user));

    public IEntity CreateCard(IEntity chest, IEntity item, int amount) {
        IEntity entity = CreateRegular(chest, item, amount);

        entity.AddComponent<NewCardItemNotificationComponent>();
        return entity;
    }
}