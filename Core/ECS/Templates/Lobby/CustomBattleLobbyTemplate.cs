using Vint.Core.Battles;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Lobby;

[ProtocolId(1498460950985)]
public class CustomBattleLobbyTemplate : BattleLobbyTemplate {
    public IEntity Create(BattleProperties battleProperties, IEntity map, IPlayerConnection owner) {
        IEntity entity = Entity(battleProperties, map);

        long price = owner.Player.IsPremium
            ? 0
            : 1000;

        entity.AddComponent(new ClientBattleParamsComponent(battleProperties));
        entity.AddComponent(new OpenCustomLobbyPriceComponent(price));
        entity.AddGroupComponent<UserGroupComponent>(owner.UserContainer.Entity);
        return entity;
    }
}
