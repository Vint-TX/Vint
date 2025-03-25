using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.PromoCodes.Events;

[ProtocolId(1490937016798)]
public class PromoCodeCheckResultEvent(
    string code,
    PromoCodeCheckResult result
) : IEvent {
    public string Code { get; private set; } = code;
    public PromoCodeCheckResult Result { get; private set; } = result;
}
