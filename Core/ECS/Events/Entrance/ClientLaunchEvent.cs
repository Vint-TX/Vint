using Serilog;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Events.Entrance;

[ProtocolId(1478774431678)]
public class ClientLaunchEvent : IServerEvent {
    public void Execute(PlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        connection.ClientSession.AddComponent(new WebIdComponent());

        logger.Debug("Executed launch event");
    }
}
