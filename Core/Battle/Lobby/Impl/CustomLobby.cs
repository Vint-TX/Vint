using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Lobby.Components;
using Vint.Core.Battle.Lobby.State;
using Vint.Core.Battle.Lobby.Templates;
using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Mode.Common.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Properties.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Maps.Components;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.User.Components;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Lobby.Impl;

public sealed class CustomLobby(
    BattleProperties properties,
    IPlayerConnection owner,
    QuestManager questManager
) : LobbyBase(questManager) {
    public IPlayerConnection Owner { get; private set; } = owner;
    public bool IsOpened { get; private set; }

    public override BattleProperties Properties { get; protected set; } = properties;
    public override IEntity Entity { get; } = new CustomBattleLobbyTemplate().Create(properties, owner);

    protected override Task PlayerJoined(LobbyPlayer player) => Task.CompletedTask;

    protected override async Task PlayerExited(LobbyPlayer player) {
        if (player.Connection != Owner) return;

        LobbyPlayer? newOwner = Players
            .ToList()
            .Shuffle()
            .FirstOrDefault();

        if (newOwner == null) return;

        await ChangeOwner(newOwner.Connection);
    }

    protected override Task RemovedFromRound(Tanker tanker) {
        LobbyPlayer lobbyPlayer = tanker.Connection.LobbyPlayer!;
        lobbyPlayer.DisposeTanker();
        return Task.CompletedTask;
    }

    public override async Task PlayerReady(LobbyPlayer player) =>
        await player.SetReady(true);

    protected override async Task RoundEnded() {
        await Round!.ModeHandler.ResetScore();
        await Entity.RemoveComponent<BattleGroupComponent>();
        await StateManager.SetState(new Awaiting(StateManager));
    }

    public override async Task Start() {
        if (StateManager.CurrentState is not Awaiting)
            return;

        await StateManager.SetState(new CustomLobbyStarting(StateManager));

        if (Round != null) {
            await Round.End();

            foreach (Tanker tanker in Round.Tankers)
                await RemoveAndDisposeTanker(tanker);

            foreach (Spectator spectator in Round.Spectators)
                await Round.RemoveSpectator(spectator);

            Round.Dispose();
            Round = null;
        }

        Round = await CreateRound();
        await Entity.AddGroupComponent<BattleGroupComponent>(Round.Entity);

        await StateManager.SetState(new Running(StateManager, Round));

        foreach (LobbyPlayer player in Players) {
            await player.SetRoundJoinTime(DateTimeOffset.UtcNow);
            await Round.AddTanker(player);
        }

        return;

        async Task RemoveAndDisposeTanker(Tanker tanker) {
            // ReSharper disable once AccessToDisposedClosure
            await Round!.RemoveTanker(tanker);
            tanker.Dispose();
        }
    }

    public async Task OpenLobby() {
        if (IsOpened) return;

        await Entity.AddComponent<OpenToConnectLobbyComponent>();
        IsOpened = true;
    }

    public async Task ChangeOwner(IPlayerConnection owner) {
        await Entity.RemoveComponent<UserGroupComponent>();
        Owner = owner;
        await Entity.AddGroupComponent<UserGroupComponent>(owner.UserContainer.Entity);
    }

    public async Task UpdateClientProperties(ClientBattleParams clientParams) {
        BattleProperties oldProperties = Properties.Clone();
        Properties.SetParams(clientParams);

        BattleMode battleMode = Properties.GetValue(BattleProperty.BattleMode);

        await Entity.RemoveComponent<MapGroupComponent>();
        await Entity.AddGroupComponent<MapGroupComponent>(Properties.GetValue(BattleProperty.MapEntity));

        await Entity.RemoveComponent<BattleModeComponent>();
        await Entity.AddComponent(new BattleModeComponent(battleMode));

        await Entity.RemoveComponent<UserLimitComponent>();
        await Entity.AddComponent(new UserLimitComponent(Properties.GetValue(BattleProperty.MaxPlayers)));

        await Entity.RemoveComponent<GravityComponent>();
        await Entity.AddComponent(new GravityComponent(Properties.GetValue(BattleProperty.Gravity)));

        await Entity.RemoveComponent<ClientBattleParamsComponent>();
        await Entity.AddComponent(new ClientBattleParamsComponent(clientParams));

        Properties.GetValue(BattleProperty.MapInfo).InitDefaultSpawnPointsIfAbsent(battleMode);
        await TeamHandler.LobbyUpdated(oldProperties, Properties);
    }
}
