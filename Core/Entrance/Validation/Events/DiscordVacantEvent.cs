using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Validation.Events;

[ProtocolId(31224)]
public class DiscordVacantEvent(
    string discordId
) : IEvent {
    public string DiscordId => discordId;
}
