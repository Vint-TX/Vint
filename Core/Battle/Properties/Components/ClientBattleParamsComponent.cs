using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Properties.Components;

[ProtocolId(1498569137147)]
public class ClientBattleParamsComponent(
    ClientBattleParams clientParams
) : IComponent {
    public ClientBattleParams Params { get; private set; } = clientParams;
}
