using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Chat.Components;

[ProtocolId(1513067769958)]
public class PersonalChatOwnerComponent(
    params List<IEntity> chats
) : PrivateComponent {
    public List<IEntity> Chats { get; private set; } = chats.ToList();
}
