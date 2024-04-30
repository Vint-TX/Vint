using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1455866538339)]
public class DiscordInvalidEvent(
    string username
) : IEvent {
    [ProtocolName("Email")] public string Username { get; private set; } = username;
}
