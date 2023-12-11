using System.Diagnostics.CodeAnalysis;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.Protocol.Commands;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public class ComponentRemoveCommand(
    IEntity entity,
    Type component
) : EntityCommand(entity) {
    [ProtocolVaried] [ProtocolPosition(1)] public Type Component { get; private set; } = component;

    public override string ToString() => $"ComponentRemove command {{ Entity: {Entity}, Component: {Component.Name} }}";
}
