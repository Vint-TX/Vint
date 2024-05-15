using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(31222)]
public class DiscordInvalidEvent(
    string discordId
) : IEvent {
    public string DiscordId => discordId;
}
