using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.User;

[ProtocolId(4788927540455272293)]
public class UserInBattleAsSpectatorComponent(
    long battleId
) : IComponent {
    public long BattleId { get; private set; } = battleId;
}