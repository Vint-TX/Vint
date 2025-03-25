using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Events;

[ProtocolId(1475648914994)]
public class CompleteBuyUsernameChangeEvent(
    bool success
) : IEvent {
    public bool Success { get; private set; } = success;
}
