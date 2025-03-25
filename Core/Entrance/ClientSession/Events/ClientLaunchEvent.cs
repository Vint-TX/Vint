using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Entrance.ClientSession.Components;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Entrance.ClientSession.Events;

[ProtocolId(1478774431678)]
public class ClientLaunchEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        ILogger logger = connection.Logger.ForType<ClientLaunchEvent>();

        await connection.ClientSession.AddComponent<WebIdComponent>();
        logger.Warning("Executed launch event");
    }
}
