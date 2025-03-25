using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Chat;

[ProtocolId(636479864244249445)]
public class SquadChatTemplate : EntityTemplate {
    public IEntity Create() => Entity("chat",
        builder => builder
            .AddComponent<ChatComponent>()
            .AddComponent(new ChatParticipantsComponent()));
}
