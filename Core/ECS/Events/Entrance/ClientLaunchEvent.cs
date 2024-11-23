using Serilog;
using Vint.Core.ECS.Components.Entrance;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance;

[ProtocolId(1478774431678)]
public class ClientLaunchEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        await connection.ClientSession.AddComponent<WebIdComponent>();
        logger.Warning("Executed launch event");
    }
}
