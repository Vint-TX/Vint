using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Rounds.Components;
using Vint.Core.Battle.Rounds.Events;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Mode.Team;

public class DominationProcessor(
    Round round,
    TeamHandler teamHandler
) {
    bool DominationStarted { get; set; }
    bool CanDominationBegin => round.Properties.GetValue(BattleProperty.Type) == BattleType.Rating &&
                               round.StateManager.CurrentState is Running &&
                               round.Elapsed.TotalMinutes >= 1 &&
                               round.Remaining.TotalMinutes >= 2;

    TeamColor DominatedTeam { get; set; }
    TimeSpan DominationDuration { get; } = TimeSpan.FromSeconds(30);
    DateTimeOffset? RoundForceEndTime { get; set; }

    public async Task ScoreUpdated() {
        if (DominationStarted)
            await TryStopDomination();
        else if (CanDominationBegin)
            await TryStartDomination();
    }

    async Task TryStartDomination() {
        if (DominationStarted) return;

        TeamColor dominatedTeam = teamHandler.GetDominatedTeamColor();
        if (dominatedTeam == TeamColor.None) return;

        DominatedTeam = dominatedTeam;
        RoundForceEndTime = DateTimeOffset.UtcNow + DominationDuration;

        await round.Entity.AddComponent(new RoundDisbalancedComponent(dominatedTeam, RoundForceEndTime.Value));
        await round.Entity.ChangeComponent<RoundStopTimeComponent>(component => component.StopTime = RoundForceEndTime.Value);

        DominationStarted = true;
    }

    async Task TryStopDomination() {
        TeamColor currentDominatedTeam = teamHandler.GetDominatedTeamColor();

        if (DominatedTeam == currentDominatedTeam)
            return;

        DominatedTeam = TeamColor.None;
        DominationStarted = false;

        await round.Players.Send<RoundBalanceRestoredEvent>();
        await round.Entity.ChangeComponent<RoundStopTimeComponent>(component => component.StopTime = round.EndTime);
        await round.Entity.RemoveComponent<RoundDisbalancedComponent>();
    }

    public async Task Tick() {
        if (!DominationStarted) return;

        if (DateTimeOffset.UtcNow >= RoundForceEndTime)
            await round.End();
    }
}
