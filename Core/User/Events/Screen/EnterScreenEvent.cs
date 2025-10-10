using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Logging;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Screen;

[ProtocolId(1453867134827)]
public class EnterScreenEvent : IServerEvent {
    public string Screen { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEntity[] entities) {
        ILogger logger = connection.Logger.ForType<EnterScreenEvent>();
        logger.Information("Entered screen {Screen}", Screen);
        return Task.CompletedTask;
    }
}
