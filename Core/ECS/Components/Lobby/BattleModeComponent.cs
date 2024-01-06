using Vint.Core.Battles;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Lobby;

[ProtocolId(1432624073184)]
public class BattleModeComponent(
    BattleMode mode
) : IComponent {
    public BattleMode BattleMode { get; private set; } = mode;
}