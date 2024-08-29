namespace Vint.Core.ECS.Components.Server.Modules;

public class TargetTierComponent : IComponent {
    public int TargetTier { get; set; }
    public int MaxExistTier { get; set; }
    public bool ContainsAllTierItem { get; set; }
    public List<long>? ItemList { get; set; }
}
