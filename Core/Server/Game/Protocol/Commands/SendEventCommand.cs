using System.Diagnostics.CodeAnalysis;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Commands;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public class SendEventCommand : IServerCommand {
    [ProtocolVaried, ProtocolPosition(0)] public required IEvent Event { get; init; }
    [ProtocolPosition(1)] public required IEntity[] Entities { get; init; }

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider) {
        ILogger logger = connection.Logger.ForType<SendEventCommand>();

        if (Event is not IServerEvent serverEvent) {
            logger.Warning("Event {Event} is not IServerEvent", Event.GetType().Name);
            return;
        }

        logger.Debug("Executing event {Name} with {Count} entities", serverEvent.GetType().Name, Entities.Length);

        await serverEvent.Execute(connection, Entities);
    }

    public override string ToString() => $"SendEvent command {{ " +
                                         $"Event: {Event.GetType().Name}, " +
                                         $"Entities: {{ {Entities.ToString(true)} }} }}";
}
