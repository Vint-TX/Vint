using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.League;

[ProtocolId(1522323975002)]
public class GetLeagueInfoEvent : IServerEvent {
    public long UserId { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) =>
        connection.Send(new UpdateTopLeagueInfoEvent(UserId), entities.Single());
}