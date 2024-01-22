using System.Diagnostics.CodeAnalysis;
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

    public override void Execute(IPlayerConnection connection) {
        IComponent component = Entity.GetComponent(Component);

        component.Removed(connection, Entity);
        Entity.RemoveComponent(Component, connection);

        connection.Logger.ForType(GetType()).Warning("Removed {Component} in {Entity}", Component.Name, Entity);
    }

    public override string ToString() => $"ComponentRemove command {{ Entity: {Entity}, Component: {Component.Name} }}";
}