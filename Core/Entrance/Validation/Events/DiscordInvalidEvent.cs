using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Validation.Events;

[ProtocolId(31222)]
public class DiscordInvalidEvent(
    string discordId
) : IEvent {
    public string DiscordId => discordId;
}
