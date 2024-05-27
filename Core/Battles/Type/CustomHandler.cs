using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
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

    public override async Task Setup() {
        Battle.MapInfo = ConfigManager.MapInfos.Single(map => map.Id == Battle.Properties.MapId);
        Battle.MapEntity = GlobalEntities.GetEntities("maps").Single(map => map.Id == Battle.Properties.MapId);
        Battle.LobbyEntity = new CustomBattleLobbyTemplate().Create(
            Battle.Properties,
            Battle.MapEntity,
            Owner);

        await Battle.StateManager.SetState(new NotStarted(Battle.StateManager));

        if (!Battle.MapInfo.HasSpawnPoints(Battle.Properties.BattleMode))
            Battle.MapInfo.InitializeDefaultSpawnPoints(Battle.Properties.BattleMode);
    }

    public override Task Tick() => Task.CompletedTask;

    public override Task PlayerEntered(BattlePlayer battlePlayer) => Task.CompletedTask;

    public override async Task PlayerExited(BattlePlayer battlePlayer) {
        if (battlePlayer.PlayerConnection != Owner) return;

        List<IPlayerConnection> players = Battle.Players
            .Where(player => !player.IsSpectator && player != battlePlayer)
            .Select(player => player.PlayerConnection)
            .ToList()
            .Shuffle();

        if (players.Count == 0) return;

        Owner = players.First();
        await Battle.LobbyEntity.RemoveComponent<UserGroupComponent>();
        await Battle.LobbyEntity.AddGroupComponent<UserGroupComponent>(Owner.User);
    }

    public async Task OpenLobby() {
        IsOpened = true;
        await Battle.LobbyEntity.AddComponent<OpenToConnectLobbyComponent>();
    }
}
