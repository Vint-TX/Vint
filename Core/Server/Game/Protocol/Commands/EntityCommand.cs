using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Server.Game.Protocol.Commands;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public abstract class EntityCommand(
    IEntity entity
) : ICommand {
    [ProtocolPosition(0)] public IEntity Entity { get; protected set; } = entity;

    public virtual Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider) => throw new UnreachableException();
}
