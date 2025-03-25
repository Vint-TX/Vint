using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Screen;

[ProtocolId(1504160222978)]
public class ChangeScreenEvent : IServerEvent {
    public string CurrentScreen { get; private set; } = null!;
    public string NextScreen { get; private set; } = null!;
    public double Duration { get; private set; }

    public Task Execute(IPlayerConnection connection, IEntity[] entities) {
        ILogger logger = connection.Logger.ForType<ChangeScreenEvent>();

        logger.Information("Changed screen {Current} to {Next}", CurrentScreen, NextScreen);
        return Task.CompletedTask;
    }
}
