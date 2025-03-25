using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Common.Components;

[ProtocolId(6549840349742289518)]
public class BattleTankCollisionsComponent : IComponent {
    public long SemiActiveCollisionsPhase { get; set; }
}
