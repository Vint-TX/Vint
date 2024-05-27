using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.League;

[ProtocolId(1522323975002)]
public class GetLeagueInfoEvent : IServerEvent {
    public long UserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) =>
        await connection.Send(new UpdateTopLeagueInfoEvent(UserId, await Leveling.GetSeasonPlace(UserId)), entities.Single());
}
