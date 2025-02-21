using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Lobby;

[ProtocolId(1495541167479)]
public class MatchMakingLobbyTemplate : BattleLobbyTemplate {
    public IEntity Create(BattleProperties properties) => Entity(properties);
}
