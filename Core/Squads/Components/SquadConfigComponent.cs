namespace Vint.Core.ECS.Components.Server.Squad;

public class SquadConfigComponent : IComponent {
    public int MaxSquadSize { get; private set; }
    public int RankRestriction { get; private set; }
}
