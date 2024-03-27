using Vint.Core.Battles;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Lobby;

[ProtocolId(1495541167479)]
public class MatchMakingLobbyTemplate : BattleLobbyTemplate {
    public IEntity Create(BattleProperties battleProperties, IEntity map) => Entity(null,
        builder => builder
            .AddComponent(new BattleModeComponent(battleProperties.BattleMode))
            .AddComponent(new UserLimitComponent(battleProperties.MaxPlayers))
            .AddComponent(new GravityComponent(battleProperties.Gravity))
            .AddComponentFrom<MapGroupComponent>(map)
            .AddGroupComponent<BattleLobbyGroupComponent>());
}