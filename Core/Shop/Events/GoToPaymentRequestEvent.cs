using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Events;

[ProtocolId(1470652819513)]
public class GoToPaymentRequestEvent : IEvent {
    public bool SteamIsActive { get; private set; }
    public string SteamId { get; private set; } = null!;
    public string Ticket { get; private set; } = null!;
}
