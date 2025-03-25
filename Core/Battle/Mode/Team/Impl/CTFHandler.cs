using System.Collections.Frozen;
using System.Numerics;
using Vint.Core.Battle.Flags;
using Vint.Core.Battle.Flags.State;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Mode.Team.Impl;

public class CTFHandler : TeamHandler {
    public CTFHandler(
        Round round,
        Func<IEntity> entityFactory,
        CreateTeamData createTeamData
    ) : base(round, createTeamData, info => info.CaptureTheFlag!.Value) {
        Entity = entityFactory();
        CTFConfig = ConfigManager.GetComponent<CtfConfigComponent>("battle/modes/ctf");

        MapFlags flagPoints = Round.Properties.MapInfo.Flags;
        TimeSpan enemyFlagActionInterval = TimeSpan.FromSeconds(CTFConfig.EnemyFlagActionMinIntervalSec);

        Flags = new Dictionary<TeamColor, Flag> {
            { TeamColor.Blue, new Flag(Round, BlueTeam.Entity, TeamColor.Blue, flagPoints.Blue, enemyFlagActionInterval) },
            { TeamColor.Red, new Flag(Round, RedTeam.Entity, TeamColor.Red, flagPoints.Red, enemyFlagActionInterval) }
        }.ToFrozenDictionary();

        CanShareFlags = Round.Properties.WarmUpDuration == TimeSpan.Zero || Round.StateManager.CurrentState is Running;
    }

    bool CanShareFlags { get; set; }
    CtfConfigComponent CTFConfig { get; }

    public FrozenDictionary<TeamColor, Flag> Flags { get; }
    public override IEntity Entity { get; }

    public override async Task Init() {
        await base.Init();

        foreach (Flag flag in Flags.Values)
            await flag.Init();
    }

    public override async Task PostPlayerJoin(BattlePlayer player) {
        await base.PostPlayerJoin(player);
        await player.Share(Flags.Values.Select(flag => flag.PedestalEntity));

        if (CanShareFlags)
            await player.Share(Flags.Values.Select(flag => flag.Entity));
    }

    public override async Task PrePlayerExit(BattlePlayer player) {
        await base.PrePlayerExit(player);
        await player.Unshare(Flags.Values.Select(flag => flag.PedestalEntity));

        if (CanShareFlags)
            await player.Unshare(Flags.Values.Select(flag => flag.Entity));

        if (player is not Tanker tanker)
            return;

        foreach (Captured captured in Flags.Values
                     .Select(flag => flag.StateManager.CurrentState)
                     .OfType<Captured>()
                     .Where(captured => captured.Carrier == tanker))
            await captured.Drop(false);
    }

    public override async Task OnWarmUpEnded() {
        await base.OnWarmUpEnded();

        if (!CanShareFlags) {
            CanShareFlags = true;
            await Round.Players.Share(Flags.Values.Select(flag => flag.Entity));
        }
    }

    public override async Task OnRoundEnded() {
        foreach (Flag flag in Flags.Values) {
            if (flag.StateManager.CurrentState is not Captured captured)
                continue;

            await captured.Drop(false);
        }

        await base.OnRoundEnded();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        foreach (Flag flag in Flags.Values)
            await flag.Tick(deltaTime);
    }

    public override TeamColor GetDominatedTeamColor() {
        const int dominationDiff = 6;
        int scoreDiff = RedTeam.Score - BlueTeam.Score;

        if (Math.Abs(scoreDiff) < dominationDiff)
            return TeamColor.None;

        return scoreDiff > 0 ? TeamColor.Blue : TeamColor.Red;
    }

    public override int CalculateReputationDelta(Tanker tanker) => tanker.TeamResult switch { // todo calculate by flags
        TeamBattleResult.Win => tanker.Connection.Player.MaxReputationDelta,
        TeamBattleResult.Draw => 0,
        TeamBattleResult.Defeat => tanker.Connection.Player.MinReputationDelta,
        _ => throw new ArgumentOutOfRangeException()
    };

    public bool CanPlaceMine(Vector3 position) =>
        Flags.Values.All(flag => Vector3.Distance(position, flag.PedestalPosition) >= CTFConfig.MinDistanceFromMineToBase);
}
