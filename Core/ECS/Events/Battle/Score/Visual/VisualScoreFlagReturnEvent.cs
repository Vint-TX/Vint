using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Score.Visual;

[ProtocolId(1511432446237)]
public class VisualScoreFlagReturnEvent(
    int score
) : VisualScoreEvent(score);
