using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type;

[ProtocolId(1505906670608), ClientAddable]
public class ForceFieldTransformComponent : IComponent {
    public ECS.Movement.Movement Movement { get; private set; }
}
