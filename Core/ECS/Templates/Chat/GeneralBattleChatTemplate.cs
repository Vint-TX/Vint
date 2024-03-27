using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Chat;

[ProtocolId(1447137441472)]
public class GeneralBattleChatTemplate : EntityTemplate {
    public IEntity Create() =>
        Entity("chat/general/ru",
            builder => builder
                .AddComponent<ChatComponent>()
                .AddComponent<GeneralBattleChatComponent>());
}