using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Chat;

[ProtocolId(636469998634338659)]
public class PersonalChatTemplate : EntityTemplate {
    public IEntity Create(IEntity sourceUser, IEntity targetUser) => Entity("chat",
        builder => builder
            .AddComponent<ChatComponent>()
            .AddComponent(new ChatParticipantsComponent(sourceUser, targetUser)));
}