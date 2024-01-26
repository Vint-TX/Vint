using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Score.Visual;

[ProtocolId(1511432334883)]
public class VisualScoreKillEvent(
    int score,
    string targetUsername,
    int targetRank
) : VisualScoreEvent(score) {
    [ProtocolName("TargetUid")] public string TargetUsername { get; } = targetUsername;
    public int TargetRank { get; } = targetRank;
}