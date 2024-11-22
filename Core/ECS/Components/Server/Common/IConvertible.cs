using JetBrains.Annotations;

namespace Vint.Core.ECS.Components.Server.Common;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
public interface IConvertible<in T> where T : IComponent {
    [UsedImplicitly]
    void Convert(T component);
}
