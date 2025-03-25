using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Reservation;

[ProtocolId(1490780962293)]
public class ReleaseReservationInBattleEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        throw new NotImplementedException();
}
