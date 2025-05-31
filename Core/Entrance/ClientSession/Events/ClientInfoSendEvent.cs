using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Logging;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.ClientSession.Events;

[ProtocolId(1464349204724)]
public class ClientInfoSendEvent : IServerEvent {
    public string Settings { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEntity[] entities) {
        ILogger logger = connection.Logger.ForType<ClientInfoSendEvent>();

        logger.Information("Client settings: {Settings}", Settings);
        return Task.CompletedTask;
    }
}
