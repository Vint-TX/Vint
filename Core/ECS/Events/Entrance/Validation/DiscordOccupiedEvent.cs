using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273457089964)]
public class DiscordOccupiedEvent(
    string username
) : IEvent {
    [ProtocolName("Email")] public string Username { get; private set; } = username;
}
