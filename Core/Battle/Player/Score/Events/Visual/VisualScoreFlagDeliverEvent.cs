using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Score.Events.Visual;

[ProtocolId(1511432397963)]
public class VisualScoreFlagDeliverEvent(
    int score
) : VisualScoreEvent(score);
