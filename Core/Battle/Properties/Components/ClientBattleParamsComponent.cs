using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle;

[ProtocolId(1498569137147)]
public class ClientBattleParamsComponent(
    ClientBattleParams clientParams
) : IComponent {
    public ClientBattleParams Params { get; private set; } = clientParams;
}
