using Vint.Core.Battle.Lobby.Components;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Properties.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Lobby.Templates;

[ProtocolId(1498460950985)]
public class CustomBattleLobbyTemplate : BattleLobbyTemplate {
    public IEntity Create(BattleProperties battleProperties, IPlayerConnection owner) {
        IEntity entity = Entity(battleProperties);

        long price = owner.Player.HasPremiumBoost ? 0 : 1000;

        entity.AddComponent(new ClientBattleParamsComponent(battleProperties.GetParams()));
        entity.AddComponent(new OpenCustomLobbyPriceComponent(price));
        entity.AddGroupComponent<UserGroupComponent>(owner.UserContainer.Entity);
        return entity;
    }
}
