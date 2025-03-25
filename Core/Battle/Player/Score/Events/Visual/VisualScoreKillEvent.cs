using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Score.Events.Visual;

[ProtocolId(1511432334883)]
public class VisualScoreKillEvent(
    int score,
    string targetUsername,
    int targetRank
) : VisualScoreEvent(score) {
    [ProtocolName("TargetUid")] public string TargetUsername { get; } = targetUsername;
    public int TargetRank { get; } = targetRank;
}
