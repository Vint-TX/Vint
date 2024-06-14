using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Reservation;

[ProtocolId(1490780962293)]
public class ReleaseReservationInBattleEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) => throw new NotImplementedException();
}
