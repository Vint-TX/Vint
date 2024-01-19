using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Chat;

[ProtocolId(1513067769958)]
public class PersonalChatOwnerComponent(
    params IEntity[] chats
) : IComponent {
    [ProtocolName("ChatIs")] public List<IEntity> Chats { get; private set; } = chats.ToList();
}