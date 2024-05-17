using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Quest;

[ProtocolId(1497606008074)]
public class UserQuestReadyEvent : IServerEvent { // todo
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.User.AddComponent<QuestsEnabledComponent>();
    }
}
