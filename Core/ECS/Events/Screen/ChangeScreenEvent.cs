using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Events.Screen;

[ProtocolId(1504160222978)]
public class ChangeScreenEvent : IServerEvent {
    public string CurrentScreen { get; private set; } = null!;
    public string NextScreen { get; private set; } = null!;
    public double Duration { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        ILogger logger = connection.Logger.ForType(GetType());

        logger.Information("{Connection} changing screen {Current} to {Next} in {Duration}",
            connection, CurrentScreen, NextScreen, Duration);
    }
}
