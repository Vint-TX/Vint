using Vint.Core.Battle.Chat.Templates;
using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Rounds.Events;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Mode.Team;

public abstract class TeamHandler : ModeHandler {
    protected TeamHandler(
        Round round,
        CreateTeamData createTeamData,
        Func<MapSpawnPointInfo, TeamSpawnPointList> getSpawnPointList
    ) : base(round) {
        BlueTeam = createTeamData(TeamColor.Blue, round, getSpawnPointList);
        RedTeam = createTeamData(TeamColor.Red, round, getSpawnPointList);

        DominationProcessor = new DominationProcessor(round, this);
    }

    public TeamData BlueTeam { get; }
    public TeamData RedTeam { get; }

    IEntity ChatEntity { get; } = new TeamBattleChatTemplate().Create();
    DominationProcessor DominationProcessor { get; }

    public override bool HasEnemies => BlueTeam.Players.Any() &&
                                       RedTeam.Players.Any();

    public override async Task Init() {
        await BlueTeam.Entity.AddGroupComponent<BattleGroupComponent>(Round.Entity);
        await RedTeam.Entity.AddGroupComponent<BattleGroupComponent>(Round.Entity);
    }

    public override async Task OnRoundEnded() {
        if (BlueTeam.Entity.Disposed || RedTeam.Entity.Disposed)
            return;

        await BlueTeam.Entity.RemoveComponent<BattleGroupComponent>();
        await RedTeam.Entity.RemoveComponent<BattleGroupComponent>();
    }

    public override async Task ResetScore() {
        await BlueTeam.ResetScore();
        await RedTeam.ResetScore();
    }

    public async Task UpdateScore(TeamColor teamColor, int delta) {
        IEntity team = teamColor switch {
            TeamColor.Blue => BlueTeam.Entity,
            TeamColor.Red => RedTeam.Entity,
            TeamColor.None => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException(nameof(teamColor), teamColor, null)
        };

        await team.ChangeComponent<TeamScoreComponent>(component => component.Score = Math.Max(0, component.Score + delta));
        await Round.Players.Send<RoundScoreUpdatedEvent>(Round.Entity);

        await DominationProcessor.ScoreUpdated();
    }

    public override SpawnPoint GetRandomSpawnPoint(Tanker tanker) => tanker.TeamColor switch {
        TeamColor.Blue => BlueTeam.GetRandomSpawnPoint(tanker),
        TeamColor.Red => RedTeam.GetRandomSpawnPoint(tanker),
        TeamColor.None => throw new InvalidOperationException(),
        _ => throw new ArgumentOutOfRangeException()
    };

    public override async Task SortAllPlayers() {
        await SortPlayers(BlueTeam.Players);
        await SortPlayers(RedTeam.Players);
    }

    public override async Task OnWarmUpEnded() {
        await base.OnWarmUpEnded();

        await UpdateScore(TeamColor.Blue, int.MinValue);
        await UpdateScore(TeamColor.Red, int.MinValue);
    }

    public override async Task PostPlayerJoin(BattlePlayer player) {
        await base.PostPlayerJoin(player);
        await player.Share(BlueTeam.Entity, RedTeam.Entity, ChatEntity);
    }

    public override async Task PostPlayerExit(BattlePlayer player) {
        await base.PostPlayerExit(player);
        await player.Unshare(BlueTeam.Entity, RedTeam.Entity, ChatEntity);
    }

    public bool IsUnfair() =>
        Math.Abs(BlueTeam.Players.Count() - RedTeam.Players.Count()) > 1;

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);
        await DominationProcessor.Tick();
    }

    public abstract TeamColor GetDominatedTeamColor();

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing)
            ChatEntity.Dispose();
    }
}
