using JetBrains.Annotations;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API.Attributes;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(IClientDTO))]
public class SubscriptionsAttribute(
    Subscriptions subscriptions
) : Attribute {
    public Subscriptions Subscriptions { get; } = subscriptions;
}
