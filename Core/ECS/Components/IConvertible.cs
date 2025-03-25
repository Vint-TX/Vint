using JetBrains.Annotations;

namespace Vint.Core.ECS.Components;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
public interface IConvertible<in T> where T : IComponent {
    [UsedImplicitly]
    void Convert(T component);
}
