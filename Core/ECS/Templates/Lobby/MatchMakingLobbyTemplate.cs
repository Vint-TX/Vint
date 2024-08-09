using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Lobby;

[ProtocolId(1495541167479)]
public class MatchMakingLobbyTemplate : BattleLobbyTemplate {
    public IEntity Create(BattleProperties battleProperties, IEntity map) =>
        Entity(battleProperties, map);
}
