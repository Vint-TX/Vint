using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Score.Events.Visual;

[ProtocolId(1511432446237)]
public class VisualScoreFlagReturnEvent(
    int score
) : VisualScoreEvent(score);
