using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Chat;

[ProtocolId(1499421322354)]
public class BattleLobbyChatTemplate : EntityTemplate {
    public IEntity Create() =>
        Entity("chat/general/ru", builder => builder.AddComponent<ChatComponent>());
}