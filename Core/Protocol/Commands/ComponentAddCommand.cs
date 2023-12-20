using System.Diagnostics.CodeAnalysis;
using Serilog;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Commands;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public class ComponentAddCommand(
    IEntity entity,
    IComponent component
) : EntityCommand(entity) {
    [ProtocolVaried]
    [ProtocolPosition(1)]
    public IComponent Component { get; private set; } = component;

    public override void Execute(IPlayerConnection connection) {
        ILogger logger = connection.Logger.ForType(GetType());

        (Entity as IInternalEntity)!.AddComponent(Component, connection);

        logger.Warning("{Connection} added {Component} to {Entity}", connection, Component.GetType().Name, Entity);
    }

    public override string ToString() =>
        $"ComponentAdd command {{ Entity: {Entity}, Component: {Component.GetType().Name} }}";
}