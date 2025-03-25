using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(635890736905417870)]
public class BattleInfoForLabelLoadedEvent(
    long mapId,
    long battleId,
    BattleMode battleMode
) : IEvent {
    public BattleInfoForLabelLoadedEvent(IEntity map, long battleId, BattleMode battleMode) : this(map.Id, battleId, battleMode) { }

    [ProtocolName("Map")] public long MapId { get; } = mapId;
    public long BattleId { get; } = battleId;
    public string BattleMode { get; } = battleMode.ToString();
}
