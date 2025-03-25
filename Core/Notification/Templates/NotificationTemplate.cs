using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Notification.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Notification.Templates;

[ProtocolId(1454656560829)]
public abstract class NotificationTemplate : EntityTemplate {
    protected IEntity Create(string configPath, IEntity? groupEntity = null) => Entity(configPath,
        builder => builder
            .AddComponent(new NotificationComponent(NotificationPriority.Message))
            .AddGroupComponent<NotificationGroupComponent>(groupEntity));
}
