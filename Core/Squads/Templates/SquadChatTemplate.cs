using Vint.Core.Chat.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Templates;

[ProtocolId(636479864244249445)]
public class SquadChatTemplate : EntityTemplate {
    public IEntity Create() => Entity("chat",
        builder => builder
            .AddComponent<ChatComponent>()
            .AddComponent(new ChatParticipantsComponent()));
}
