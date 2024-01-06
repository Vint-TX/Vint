using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle;

[ProtocolId(6549840349742289518)]
public class BattleTankCollisionsComponent : IComponent {
    public long SemiActiveCollisionsPhase { get; set; }
}