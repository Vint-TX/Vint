using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Score.Visual;

[ProtocolId(1511432397963)]
public class VisualScoreFlagDeliverEvent(
    int score
) : VisualScoreEvent(score);