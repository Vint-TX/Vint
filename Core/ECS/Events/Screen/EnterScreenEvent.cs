using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Screen;

[ProtocolId(1453867134827)]
public class EnterScreenEvent : IServerEvent {
    public string Screen { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());
        logger.Information("Entered screen {Screen}", Screen);
        return Task.CompletedTask;
    }
}
