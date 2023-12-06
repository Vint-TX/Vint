using Vint.Core.ECS.Entities;

namespace Vint.Core.Protocol.Commands;

public class EntityUnshareCommand(IEntity entity) : EntityCommand(entity);