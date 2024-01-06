using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Payment;

[ProtocolId(1471252962981)]
public class PaymentStatisticsEvent : IEvent {
    public PaymentStatisticsAction Action { get; private set; }
    public long Item { get; private set; }
    public long Method { get; private set; }
    public string Screen { get; private set; } = null!;
}