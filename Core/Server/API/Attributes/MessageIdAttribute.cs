using JetBrains.Annotations;
using Vint.Core.Server.API.DTO.Base;

namespace Vint.Core.Server.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[MeansImplicitUse(ImplicitUseKindFlags.Access)]
[BaseTypeRequired(typeof(IClientDTO))]
public class MessageIdAttribute(
    int id
) : Attribute {
    public int Id { get; } = id;
}
