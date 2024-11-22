using JetBrains.Annotations;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events;

public interface IEvent;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IServerEvent : IEvent {
    Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities);
}
