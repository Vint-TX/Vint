using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Settings;

[ProtocolId(1475648914994)]
public class CompleteBuyUsernameChangeEvent(
    bool success
) : IEvent {
    public bool Success { get; private set; } = success;
}