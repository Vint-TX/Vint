using Vint.Core.ECS.Components;

namespace Vint.Core.Squads.Components;

public class SquadConfigComponent : IComponent {
    public int MaxSquadSize { get; private set; }
    public int RankRestriction { get; private set; }
}
