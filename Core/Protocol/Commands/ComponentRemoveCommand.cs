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
public class ComponentRemoveCommand(
    IEntity entity,
    Type component
) : EntityCommand(entity) {
    [ProtocolVaried, ProtocolPosition(1)] public Type Component { get; private set; } = component;

    public override async Task Execute(IPlayerConnection connection) {
        ILogger logger = connection.Logger.ForType(GetType());
        ClientRemovableAttribute? clientRemovable = Component.GetCustomAttribute<ClientRemovableAttribute>();

        if (clientRemovable == null) {
            logger.Error("{Component} is not in whitelist ({Entity})", Component.Name, Entity);
            /*ChatUtils.SendMessage($"ClientRemovable: {Component.Name}", ChatUtils.GetChat(connection), [connection], null);*/
            return; // maybe disconnect
        }

        IComponent component = Entity.GetComponent(Component);

        await Entity.RemoveComponent(Component, connection);
        await component.Removed(connection, Entity);

        logger.Debug("Removed {Component} from {Entity}", Component.Name, Entity);
    }

    public override string ToString() =>
        $"ComponentRemove command {{ Entity: {Entity}, Component: {Component.Name} }}";
}
