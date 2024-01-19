using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Chat;

[ProtocolId(636437655901996504)]
public class ChatParticipantsComponent(
    params IEntity[] participants
) : IComponent {
    public List<IEntity> Users { get; private set; } = participants.ToList();
}