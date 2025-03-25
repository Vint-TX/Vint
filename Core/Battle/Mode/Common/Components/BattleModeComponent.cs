using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Mode.Common.Components;

[ProtocolId(1432624073184)]
public class BattleModeComponent(
    BattleMode mode
) : IComponent {
    public BattleMode BattleMode { get; private set; } = mode;
}
