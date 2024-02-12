using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Score.Visual;

[ProtocolId(1511432353980)]
public class VisualScoreAssistEvent(
    int score,
    int percent,
    string targetUsername
) : VisualScoreEvent(score) {
    [ProtocolName("TargetUid")] public string TargetUsername { get; } = targetUsername;
    public int Percent { get; } = percent;
}