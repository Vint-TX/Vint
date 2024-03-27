using Vint.Core.Battles;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Templates.Lobby;

[ProtocolId(1498460950985)]
public class CustomBattleLobbyTemplate : BattleLobbyTemplate {
    public IEntity Create(BattleProperties battleProperties, IEntity map, IPlayerConnection owner) => Entity(null,
        builder => builder
            .AddComponent(new BattleModeComponent(battleProperties.BattleMode))
            .AddComponent(new UserLimitComponent(battleProperties.MaxPlayers))
            .AddComponent(new GravityComponent(battleProperties.Gravity))
            .AddGroupComponent<UserGroupComponent>(owner.User)
            .AddComponent(new ClientBattleParamsComponent(battleProperties))
            .AddComponent(new OpenCustomLobbyPriceComponent(owner.Player.IsPremium ? 0 : 1000))
            .AddComponentFrom<MapGroupComponent>(map)
            .AddGroupComponent<BattleLobbyGroupComponent>());
}