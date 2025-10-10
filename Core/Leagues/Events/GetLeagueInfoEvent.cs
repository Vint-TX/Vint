using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Leagues.Events;

[ProtocolId(1522323975002)]
public class GetLeagueInfoEvent : IServerEvent {
    public long UserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        await connection.Send(new UpdateTopLeagueInfoEvent(UserId, await Leveling.GetSeasonPlace(UserId)), connection.UserContainer.Entity);
}
