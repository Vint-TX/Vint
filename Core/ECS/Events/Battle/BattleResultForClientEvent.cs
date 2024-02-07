using Vint.Core.Battles.Results;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(1510029455297)]
public class BattleResultForClientEvent(
    BattleResultForClient result
) : IEvent {
    public BattleResultForClient UserResultForClient { get; private set; } = result;
}