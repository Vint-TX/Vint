using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Type;

public class CustomHandler(
    Battle battle,
    IPlayerConnection owner
) : TypeHandler(battle) {
    public IPlayerConnection Owner { get; private set; } = owner;

    public bool IsOpened { get; private set; }

    public override void Setup() {
        Battle.MapInfo = ConfigManager.MapInfos.Values.Single(map => map.MapId == Battle.Properties.MapId);
        Battle.MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == Battle.Properties.MapId);
        Battle.LobbyEntity = new CustomBattleLobbyTemplate().Create(
            Battle.Properties,
            Battle.MapEntity,
            BattleProperties.GravityToForce[Battle.Properties.Gravity],
            Owner);
    }

    public override void Tick() { }

    public override void PlayerEntered(BattlePlayer battlePlayer) { }

    public override void PlayerExited(BattlePlayer battlePlayer) {
        if (battlePlayer.PlayerConnection != Owner) return;

        List<IPlayerConnection> players = Battle.Players
            .Where(player => !player.IsSpectator && player != battlePlayer)
            .Select(player => player.PlayerConnection)
            .ToList()
            .Shuffle();

        if (players.Count == 0) return;

        Owner = players.First();
        Battle.LobbyEntity.RemoveComponent<UserGroupComponent>();
        Battle.LobbyEntity.AddComponent(new UserGroupComponent(Owner.User));
    }

    public void OpenLobby() {
        IsOpened = true;
        Battle.LobbyEntity.AddComponent(new OpenToConnectLobbyComponent());
    }
}