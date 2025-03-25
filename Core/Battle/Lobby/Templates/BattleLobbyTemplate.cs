using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Lobby;

[ProtocolId(1498460800928)]
public class BattleLobbyTemplate : EntityTemplate {
    protected IEntity Entity(BattleProperties properties) => Entity(null,
        builder => builder
            .AddComponent(new BattleModeComponent(properties.BattleMode))
            .AddComponent(new UserLimitComponent(properties.MaxPlayers))
            .AddComponent(new GravityComponent(properties.Gravity))
            .AddGroupComponent<MapGroupComponent>(properties.MapEntity)
            .AddGroupComponent<BattleLobbyGroupComponent>());
}
