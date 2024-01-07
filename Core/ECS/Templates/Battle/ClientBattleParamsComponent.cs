using Vint.Core.Battles;
using Vint.Core.ECS.Components;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle;

[ProtocolId(1498569137147)]
public class ClientBattleParamsComponent(
    BattleProperties properties
) : IComponent {
    public BattleProperties Params { get; private set; } = properties;
}