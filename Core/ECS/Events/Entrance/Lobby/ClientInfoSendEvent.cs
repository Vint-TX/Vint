using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Entrance.Lobby;

[ProtocolId(1464349204724)]
public class ClientInfoSendEvent : IServerEvent {
    public string Settings { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("Client settings: {Settings}", Settings);
        return Task.CompletedTask;
    }
}
