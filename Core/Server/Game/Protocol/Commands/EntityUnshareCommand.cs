using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Server.Game.Protocol.Commands;

public class EntityUnshareCommand(
    IEntity entity
) : ICommand {
    [ProtocolPosition(0)] public IEntity Entity => entity;

    public override string ToString() => $"EntityUnshare command {{ Entity: {Entity} }}";
}
