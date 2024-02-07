using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
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

        CanShareFlags = Battle.IsCustom;
    }

    bool CanShareFlags { get; set; }

    public IReadOnlyDictionary<TeamColor, Flag> Flags { get; }

    protected override List<SpawnPoint> RedSpawnPoints { get; }
    protected override List<SpawnPoint> BlueSpawnPoints { get; }

    public override void OnStarted() { }

    public override void OnWarmUpCompleted() {
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

    public override int CalculateReputationDelta(BattlePlayer player) => 0; // todo calculate by flags

    public override void Tick() {
        foreach (Flag flag in Flags.Values)
            flag.StateManager.Tick();
    }
}