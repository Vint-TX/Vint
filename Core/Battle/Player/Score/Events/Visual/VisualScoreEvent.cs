using Vint.Core.ECS.Events;

namespace Vint.Core.Battle.Player.Score.Events.Visual;

public abstract class VisualScoreEvent(
    int score
) : IEvent {
    public int Score { get; } = score;
}
