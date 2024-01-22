using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(635890736905417870)]
public class BattleInfoForLabelLoadedEvent(
    IEntity map,
    long battleId,
    BattleMode battleMode
) : IEvent {
    public IEntity Map { get; private set; } = map;
    public long BattleId { get; private set; } = battleId;
    public string BattleMode { get; private set; } = battleMode.ToString();
}