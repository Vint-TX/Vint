using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1454656560829)]
public abstract class NotificationTemplate : EntityTemplate {
    protected IEntity Create(string configPath) {
        IEntity entity = Entity(configPath,
            builder =>
                builder
                    .AddComponent(new NotificationComponent(NotificationPriority.Message)));

        entity.AddComponent(new NotificationGroupComponent(entity));
        return entity;
    }
}