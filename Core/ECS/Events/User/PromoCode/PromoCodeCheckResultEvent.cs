using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.PromoCode;

[ProtocolId(1490937016798)]
public class PromoCodeCheckResultEvent(
    string code,
    PromoCodeCheckResult result
) : IEvent {
    public string Code { get; private set; } = code;
    public PromoCodeCheckResult Result { get; private set; } = result;
}