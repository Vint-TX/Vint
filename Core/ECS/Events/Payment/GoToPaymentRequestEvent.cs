using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Payment;

[ProtocolId(1470652819513)]
public class GoToPaymentRequestEvent : IEvent {
    public bool SteamIsActive { get; private set; }
    public string SteamId { get; private set; } = null!;
    public string Ticket { get; private set; } = null!;
}