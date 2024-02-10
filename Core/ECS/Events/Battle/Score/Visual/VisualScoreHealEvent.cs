using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Score.Visual;

[ProtocolId(1511846568255)]
public class VisualScoreHealEvent(
    int score
) : VisualScoreEvent(score);