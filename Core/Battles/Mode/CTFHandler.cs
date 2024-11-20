using System.Collections.Frozen;
using System.Numerics;
using Vint.Core.Battles.Flags;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Server.Battle;
using Vint.Core.ECS.Enums;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public class CTFHandler : TeamHandler {
    public CTFHandler(Battle battle) : base(battle) {
        RedSpawnPoints = Battle.MapInfo.SpawnPoints.CaptureTheFlag!.Value.RedTeam.ToList();
        BlueSpawnPoints = Battle.MapInfo.SpawnPoints.CaptureTheFlag!.Value.BlueTeam.ToList();
        CTFConfig = ConfigManager.GetComponent<CtfConfigComponent>("battle/modes/ctf");

        TimeSpan enemyFlagActionInterval = TimeSpan.FromSeconds(CTFConfig.EnemyFlagActionMinIntervalSec);

        Flags = new HashSet<Flag> {
            new(Battle, RedTeam, TeamColor.Red, Battle.MapInfo.Flags.Red, enemyFlagActionInterval),
            new(Battle, BlueTeam, TeamColor.Blue, Battle.MapInfo.Flags.Blue, enemyFlagActionInterval)
        }.ToFrozenSet();

        CanShareFlags = Battle.TypeHandler is not MatchmakingHandler;
    }

    bool CanShareFlags { get; set; }
    CtfConfigComponent CTFConfig { get; }

    public FrozenSet<Flag> Flags { get; }

    protected override List<SpawnPoint> RedSpawnPoints { get; }
    protected override List<SpawnPoint> BlueSpawnPoints { get; }

    public override async Task OnWarmUpCompleted() {
        await base.OnWarmUpCompleted();
        CanShareFlags = true;

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
            await battlePlayer.PlayerConnection.Share(Flags.Select(flag => flag.Entity));
    }

    public override async Task OnFinished() {
        foreach (Flag flag in Flags)
            await flag.Drop(false);
    }

    public override async Task PlayerEntered(BattlePlayer player) {
        await base.PlayerEntered(player);

        await player.PlayerConnection.Share(Flags.Select(flag => flag.PedestalEntity));

        if (CanShareFlags)
            await player.PlayerConnection.Share(Flags.Select(flag => flag.Entity));
    }

    public override async Task PlayerExited(BattlePlayer player) {
        await base.PlayerExited(player);

        await player.PlayerConnection.UnshareIfShared(Flags.SelectMany(flag => new[] { flag.PedestalEntity, flag.Entity }));

        foreach (Flag flag in Flags.Where(flag => flag.Carrier == player))
            await flag.Drop(false);
    }

    public override TeamColor GetDominatedTeam() {
        const int dominationDiff = 6;

        TeamColor dominationTeam = TeamColor.None;
        int redScore = RedTeam.GetComponent<TeamScoreComponent>().Score;
        int blueScore = BlueTeam.GetComponent<TeamScoreComponent>().Score;
        int diff = Math.Abs(redScore - blueScore);

        if (diff >= dominationDiff)
            dominationTeam = redScore > blueScore ? TeamColor.Blue : TeamColor.Red;

        return dominationTeam;
    }

    public override int CalculateReputationDelta(BattlePlayer player) => player.TeamBattleResult switch { // todo calculate by flags
        TeamBattleResult.Win => player.PlayerConnection.Player.MaxReputationDelta,
        TeamBattleResult.Draw => 0,
        TeamBattleResult.Defeat => player.PlayerConnection.Player.MinReputationDelta,
        _ => -99999
    };

    public override async Task Tick(TimeSpan deltaTime) {
        foreach (Flag flag in Flags)
            await flag.StateManager.Tick(deltaTime);
    }

    public bool CanPlaceMine(Vector3 position) =>
        Flags.All(flag => Vector3.Distance(position, flag.PedestalPosition) >= CTFConfig.MinDistanceFromMineToBase);
}
