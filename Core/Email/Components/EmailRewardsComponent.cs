using JetBrains.Annotations;
using Vint.Core.ECS.Components;

namespace Vint.Core.Email.Components;

public class EmailRewardsComponent : IComponent {
    public List<EmailReward> Rewards { get; private set; } = null!;
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class EmailReward {
    public long Id { get; set; }
    public int Amount { get; set; }
}
