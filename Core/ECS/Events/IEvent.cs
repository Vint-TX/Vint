using JetBrains.Annotations;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events;

public interface IEvent;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IServerEvent : IEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities);
}
