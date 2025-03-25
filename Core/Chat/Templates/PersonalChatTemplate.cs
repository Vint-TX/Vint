using Vint.Core.Chat.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Chat.Templates;

[ProtocolId(636469998634338659)]
public class PersonalChatTemplate : EntityTemplate {
    public IEntity Create(IEntity sourceUser, IEntity targetUser) => Entity("chat",
        builder => builder
            .AddComponent<ChatComponent>()
            .AddComponent(new ChatParticipantsComponent(sourceUser, targetUser)));
}
