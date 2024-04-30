using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273700499964)]
public class DiscordVacantEvent(
    string username
) : IEvent {
    [ProtocolName("Email")] public string Username { get; private set; } = username;
}
