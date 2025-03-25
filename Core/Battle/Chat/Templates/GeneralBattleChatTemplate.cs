using Vint.Core.Battle.Chat.Components;
using Vint.Core.Chat.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Chat.Templates;

[ProtocolId(1447137441472)]
public class GeneralBattleChatTemplate : EntityTemplate {
    public IEntity Create() =>
        Entity("chat/general/ru",
            builder => builder
                .AddComponent<ChatComponent>()
                .AddComponent<GeneralBattleChatComponent>());
}
