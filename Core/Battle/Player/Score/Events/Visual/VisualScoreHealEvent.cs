using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Score.Events.Visual;

[ProtocolId(1511846568255)]
public class VisualScoreHealEvent(
    int score
) : VisualScoreEvent(score);
