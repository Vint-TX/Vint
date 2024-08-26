using JetBrains.Annotations;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(BattleModule))]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class ModuleIdAttribute(
    long id
) : Attribute {
    public long Id { get; } = id;
}
