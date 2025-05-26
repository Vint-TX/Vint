using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Events;

[ProtocolId(1459256177890)]
public class EmailNotConfirmedEvent : IEvent {
    [Obsolete("Not used in client anymore; will be removed in the next client update")]
    public string Email => "";
}
