using System.Diagnostics.CodeAnalysis;
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

    public override void Execute(IPlayerConnection connection) {
        Entity.ChangeComponent(Component, connection);
        Component.Changed(connection, Entity);

        connection.Logger.ForType(GetType()).Warning("Changed {Component} in {Entity}", Component.GetType().Name, Entity);
    }

    public override string ToString() =>
        $"ComponentChange command {{ Entity: {Entity}, Component: {Component.GetType().Name} }}";
}