using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Modules.Common.Components;

public class TargetTierComponent : IComponent {
    public int TargetTier { get; set; }
    public int MaxExistTier { get; set; }
    public bool ContainsAllTierItem { get; set; }
    public List<long>? ItemList { get; set; }
}
