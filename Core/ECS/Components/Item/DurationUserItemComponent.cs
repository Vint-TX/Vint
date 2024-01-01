using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Item;

[ProtocolId(1513581047619)]
public class DurationUserItemComponent(
    DateTimeOffset? endTime = null
) : IComponent {
    public DateTimeOffset EndTime { get; private set; } = endTime ?? DateTimeOffset.UtcNow.AddHours(6);
}