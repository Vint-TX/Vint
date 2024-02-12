using Vint.Core.Battles.Flags;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Enums;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public class CTFHandler : TeamHandler {
    public CTFHandler(Battle battle) : base(battle) {
        RedSpawnPoints = Battle.MapInfo.SpawnPoints.CaptureTheFlag.RedTeam.ToList();
        BlueSpawnPoints = Battle.MapInfo.SpawnPoints.CaptureTheFlag.BlueTeam.ToList();

        Flags = new Dictionary<TeamColor, Flag> {
            { TeamColor.Red, new Flag(Battle, RedTeam, TeamColor.Red, Battle.MapInfo.Flags.Red) },
            { TeamColor.Blue, new Flag(Battle, BlueTeam, TeamColor.Blue, Battle.MapInfo.Flags.Blue) }
        };

        CanShareFlags = Battle.TypeHandler is not MatchmakingHandler;
    }

    bool CanShareFlags { get; set; }

    public IReadOnlyDictionary<TeamColor, Flag> Flags { get; }

    protected override List<SpawnPoint> RedSpawnPoints { get; }
    protected override List<SpawnPoint> BlueSpawnPoints { get; }

    public override void OnWarmUpCompleted() {
        base.OnWarmUpCompleted();
        CanShareFlags = true;

        foreach (BattlePlayer battlePlayer in Battle.Players.ToList().Where(battlePlayer => battlePlayer.InBattle))
            battlePlayer.PlayerConnection.Share(Flags.Values.Select(flag => flag.Entity));
    }

    public override void OnFinished() { }

    public override void PlayerEntered(BattlePlayer player) {
        base.PlayerEntered(player);

        player.PlayerConnection.Share(Flags.Values.Select(flag => flag.PedestalEntity));

        if (CanShareFlags)
            player.PlayerConnection.Share(Flags.Values.Select(flag => flag.Entity));
    }

    public override void PlayerExited(BattlePlayer player) {
        base.PlayerExited(player);

        foreach (Flag flag in Flags.Values.Where(flag => flag.Carrier == player))
            flag.Drop(false);

        player.PlayerConnection.UnshareIfShared(Flags.Values.SelectMany(flag => new[] { flag.PedestalEntity, flag.Entity }));
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

    public override void Tick() {
        foreach (Flag flag in Flags.Values)
            flag.StateManager.Tick();
    }
}