using System.Diagnostics.CodeAnalysis;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Commands;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public class SendEventCommand(
    IEvent @event,
    params IEntity[] entities
) : ICommand {
    [ProtocolVaried, ProtocolPosition(0)] public IEvent Event { get; private set; } = @event;
    [ProtocolPosition(1)] public IEntity[] Entities { get; private set; } = entities;

    public async Task Execute(IPlayerConnection connection) {
        ILogger logger = connection.Logger.ForType(GetType());

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
