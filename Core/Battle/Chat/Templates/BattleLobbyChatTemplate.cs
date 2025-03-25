using Vint.Core.Chat.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Chat.Templates;

[ProtocolId(1499421322354)]
public class BattleLobbyChatTemplate : EntityTemplate {
    public IEntity Create() =>
        Entity("chat/general/ru", builder => builder.AddComponent<ChatComponent>());
}
