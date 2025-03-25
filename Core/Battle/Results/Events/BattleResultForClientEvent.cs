using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Results.Events;

[ProtocolId(1510029455297)]
public class BattleResultForClientEvent(
    BattleResultForClient result
) : IEvent {
    public BattleResultForClient UserResultForClient { get; private set; } = result;
}
