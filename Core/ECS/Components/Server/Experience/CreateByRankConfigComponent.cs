namespace Vint.Core.ECS.Components.Server.Experience;

public class CreateByRankConfigComponent : IComponent {
    public List<int> UserRankListToCreateItem { get; private set; } = null!;
}
