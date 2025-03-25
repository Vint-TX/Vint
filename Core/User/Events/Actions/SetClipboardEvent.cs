using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Actions;

[ProtocolId(31217)]
public class SetClipboardEvent(
    string content
) : IEvent {
    public string Content { get; private set; } = content;
}
