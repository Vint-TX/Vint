using JetBrains.Annotations;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[BaseTypeRequired(typeof(IClientDTO))]
[MeansImplicitUse]
public class MessageIdAttribute(
    int id
) : Attribute {
    public int Id { get; } = id;
}
