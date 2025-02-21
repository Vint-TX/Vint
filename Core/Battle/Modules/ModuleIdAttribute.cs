using JetBrains.Annotations;
using Vint.Core.Battle.Modules.Types.Base;

namespace Vint.Core.Battle.Modules;

[AttributeUsage(AttributeTargets.Class), BaseTypeRequired(typeof(BattleModule)),
 MeansImplicitUse(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class ModuleIdAttribute(
    long id
) : Attribute {
    public long Id { get; } = id;
}
