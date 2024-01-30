using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle;

[ProtocolId(1436532217083)]
public class BattleScoreComponent : IComponent {
    public int Score { get; private set; }
    public int ScoreRed { get; private set; }
    public int ScoreBlue { get; private set; }
}