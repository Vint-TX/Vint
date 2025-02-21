using System.Diagnostics.CodeAnalysis;
using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Matchmaking;
using Vint.Core.Server.Game;
using Running = Vint.Core.Battle.Lobby.Running;

namespace Vint.Core.Battle.Player;

public class LobbyPlayer(
    IPlayerConnection connection,
    LobbyBase lobby
) : IWithConnection, IDisposable {
    public Guid Id { get; } = Guid.NewGuid();

    public IPlayerConnection Connection { get; } = connection;
    public LobbyBase Lobby { get; } = lobby;
    public Round? Round => Lobby.StateManager.CurrentState is Running running ? running.Round : null;

    public bool Ready { get; private set; }

    [MemberNotNullWhen(true, nameof(Tanker), nameof(Round))]
    public bool InRound => Tanker != null;
    public Tanker? Tanker { get; private set; }

    public DateTimeOffset? RoundJoinTime { get; private set; }

    public IEntity? Team { get; private set; }
    public TeamColor TeamColor { get; private set; } = TeamColor.None;

    [MemberNotNull(nameof(Tanker))]
    public async Task JoinRound(Round round) {
        await SetReady(false);

        Tanker = new Tanker(round, Connection, Team);
        await Tanker.Init();

        foreach (Spectator spectator in round.Spectators)
            await Connection.UserContainer.ShareTo(spectator.Connection);
    }

    public async Task SetTeam(IEntity? team) {
        TeamColor color = team?.GetComponent<TeamColorComponent>().TeamColor ?? TeamColor.None;
        IEntity user = Connection.UserContainer.Entity;

        Team = team;
        TeamColor = color;

        await user.RemoveComponentIfPresent<TeamColorComponent>();
        await user.AddComponent(new TeamColorComponent(color));
    }

    public async Task SetReady(bool isReady) {
        if (Ready == isReady)
            return;

        Ready = isReady;
        IEntity user = Connection.UserContainer.Entity;

        if (Ready) await user.AddComponent<MatchMakingUserReadyComponent>();
        else await user.RemoveComponent<MatchMakingUserReadyComponent>();
    }

    public async Task SetRoundJoinTime(DateTimeOffset time) {
        RoundJoinTime = time;
        await Connection.Send(new MatchMakingLobbyStartTimeEvent(time), Connection.UserContainer.Entity);
    }

    public void DisposeTanker() {
        Tanker?.Dispose();
        Tanker = null;
    }

    void Dispose(bool disposing) {
        if (disposing) { // todo dispose entities
            DisposeTanker();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~LobbyPlayer() => Dispose(false);
}
