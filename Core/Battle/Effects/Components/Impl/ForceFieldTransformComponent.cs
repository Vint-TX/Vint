using Vint.Core.Battle.Tank.Movement;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl;

[ProtocolId(1505906670608), ClientAddable]
public class ForceFieldTransformComponent : IComponent {
    public Movement Movement { get; private set; }
}
