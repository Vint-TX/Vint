using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Validation.Events;

[ProtocolId(31223)]
public class DiscordOccupiedEvent(
    string discordId
) : IEvent {
    public string DiscordId => discordId;
}
