using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Battle.Reservation;

[ProtocolId(1490780962293)]
public class ReleaseReservationInBattleEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) =>
        throw new NotImplementedException();
}
