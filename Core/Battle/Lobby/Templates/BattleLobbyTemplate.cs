using Vint.Core.Battle.Lobby.Components;
using Vint.Core.Battle.Mode.Common.Components;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Maps.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.Templates;

[ProtocolId(1498460800928)]
public class BattleLobbyTemplate : EntityTemplate {
    protected IEntity Entity(BattleProperties properties) => Entity(null,
        builder => builder
            .AddComponent(new BattleModeComponent(properties.GetValue(BattleProperty.BattleMode)))
            .AddComponent(new UserLimitComponent(properties.GetValue(BattleProperty.MaxPlayers)))
            .AddComponent(new GravityComponent(properties.GetValue(BattleProperty.Gravity)))
            .AddGroupComponent<MapGroupComponent>(properties.GetValue(BattleProperty.MapEntity))
            .AddGroupComponent<BattleLobbyGroupComponent>());
}
