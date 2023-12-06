using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Events.Screen;

[ProtocolId(1453867134827)]
public class EnterScreenEvent : IServerEvent {
    public string Screen { get; private set; } = null!;

    public void Execute(PlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("{Connection} entered screen {Screen}", connection, Screen);
    }
}
