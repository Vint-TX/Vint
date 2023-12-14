using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events;

public interface IEvent;

public interface IServerEvent : IEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities);
}