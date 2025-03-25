using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Lobby;

[ProtocolId(1432624073184)]
public class BattleModeComponent(
    BattleMode mode
) : IComponent {
    public BattleMode BattleMode { get; private set; } = mode;
}
