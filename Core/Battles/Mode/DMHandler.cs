using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Mode;

public class DMHandler(
    Battle battle
) : SoloHandler(battle) {
    protected override List<SpawnPoint> SpawnPoints { get; } = battle.MapInfo.SpawnPoints.Deathmatch.ToList();

    public override int CalculateReputationDelta(BattlePlayer battlePlayer) {
        List<BattlePlayer> players = Battle.Players.ToList()
            .Where(player => player.InBattleAsTank)
            .OrderBy(player => player.Tank!.DealtDamage)
            .ToList();

        Database.Models.Player player = battlePlayer.PlayerConnection.Player;
        return players.Count < 2
                   ? 0
                   : MathUtils.Map(players.IndexOf(battlePlayer) + 1, 1, players.Count, player.MaxReputationDelta, player.MinReputationDelta);
    }
}