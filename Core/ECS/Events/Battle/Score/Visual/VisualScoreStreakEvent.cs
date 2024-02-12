using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Score.Visual;

[ProtocolId(1512478367453)]
public class VisualScoreStreakEvent(
    int score
) : VisualScoreEvent(score);