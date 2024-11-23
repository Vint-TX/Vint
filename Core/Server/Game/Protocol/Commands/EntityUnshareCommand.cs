using Vint.Core.ECS.Entities;

namespace Vint.Core.Server.Game.Protocol.Commands;

public class EntityUnshareCommand(
    IEntity entity
) : EntityCommand(entity) {
    public override string ToString() => $"EntityUnshare command {{ Entity: {Entity} }}";
}
