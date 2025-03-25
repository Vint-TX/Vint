using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Score.Events.Visual;

[ProtocolId(1512478367453)]
public class VisualScoreStreakEvent(
    int score
) : VisualScoreEvent(score);
