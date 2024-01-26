namespace Vint.Core.ECS.Events.Battle.Score.Visual;

public abstract class VisualScoreEvent(
    int score
) : IEvent {
    public int Score { get; } = score;
}