using Vint.Core.Battles;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Lobby;

[ProtocolId(1498460800928)]
public class BattleLobbyTemplate : EntityTemplate {
    protected IEntity Entity(BattleProperties properties, IEntity map) => Entity(null,
        builder => builder
            .AddComponent(new BattleModeComponent(properties.BattleMode))
            .AddComponent(new UserLimitComponent(properties.MaxPlayers))
            .AddComponent(new GravityComponent(properties.Gravity))
            .AddComponentFrom<MapGroupComponent>(map)
            .AddGroupComponent<BattleLobbyGroupComponent>());
}
