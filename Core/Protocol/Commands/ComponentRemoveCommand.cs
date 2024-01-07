using System.Diagnostics.CodeAnalysis;
using Serilog;
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
    [ProtocolIgnore] static ILogger Logger { get; } = Log.Logger.ForType(typeof(ComponentRemoveCommand));
    [ProtocolVaried, ProtocolPosition(1)] public Type Component { get; private set; } = component;

    public override void Execute(IPlayerConnection connection) {
        Entity.RemoveComponent(Component, connection);

        Logger.Warning("{Connection} removed {Component} in {Entity}", connection, Component.Name, Entity);
    }

    public override string ToString() => $"ComponentRemove command {{ Entity: {Entity}, Component: {Component.Name} }}";
}