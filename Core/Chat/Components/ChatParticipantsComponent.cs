using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Chat.Components;

[ProtocolId(636437655901996504)]
public class ChatParticipantsComponent(
    params List<IEntity> participants
) : IComponent {
    public List<IEntity> Users { get; set; } = participants.ToList();
}
