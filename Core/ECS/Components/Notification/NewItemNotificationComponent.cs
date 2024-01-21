using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Notification;

[ProtocolId(1481177510866)]
public class NewItemNotificationComponent(
    IEntity item,
    int amount
) : IComponent {
    public IEntity Item { get; private set; } = item;
    public int Amount { get; private set; } = amount;
}