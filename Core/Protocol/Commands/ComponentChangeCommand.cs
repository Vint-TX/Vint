using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Serilog;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Commands;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public class ComponentChangeCommand(
    IEntity entity,
    IComponent component
) : EntityCommand(entity) {
    [ProtocolVaried, ProtocolPosition(1)] public IComponent Component { get; private set; } = component;

    public override async Task Execute(IPlayerConnection connection) {
        ILogger logger = connection.Logger.ForType(GetType());
        Type type = Component.GetType();
        ClientChangeableAttribute? clientChangeable = type.GetCustomAttribute<ClientChangeableAttribute>();

        if (clientChangeable == null) {
            logger.Error("{Component} is not in whitelist ({Entity})", type.Name, Entity);
            /*ChatUtils.SendMessage($"ClientChangeable: {type.Name}", ChatUtils.GetChat(connection), [connection], null);*/
            return; // maybe disconnect
        }

        await Entity.ChangeComponent(Component, connection);
        await Component.Changed(connection, Entity);

        logger.Debug("Changed {Component} in {Entity}", type.Name, Entity);
    }

    public override string ToString() =>
        $"ComponentChange command {{ Entity: {Entity}, Component: {Component.GetType().Name} }}";
}
