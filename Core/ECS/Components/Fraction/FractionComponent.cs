using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Fraction;

[ProtocolId(1544499423535)]
public class FractionComponent(
    string name
) : IComponent {
    public string Name { get; private set; } = name;
}