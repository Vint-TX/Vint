using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Reservation;

[ProtocolId(1490604380473)]
public class ReturnToBattleEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) => throw new NotImplementedException();
}
