using JetBrains.Annotations;

namespace Vint.Core.Server.API.Attributes;

[AttributeUsage(AttributeTargets.Class), MeansImplicitUse(ImplicitUseKindFlags.Access)]
public class MessageIdAttribute(
    int id
) : Attribute {
    public int Id { get; } = id;
}
