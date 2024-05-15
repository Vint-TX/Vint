using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(31224)]
public class DiscordVacantEvent(
    string discordId
) : IEvent {
    public string DiscordId => discordId;
}
