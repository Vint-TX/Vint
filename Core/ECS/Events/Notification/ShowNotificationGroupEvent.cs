using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Notification;

[ProtocolId(1487160556894)]
public class ShowNotificationGroupEvent(
    int expectedMembersCount
) : IEvent {
    public int ExpectedMembersCount { get; private set; } = expectedMembersCount;
}