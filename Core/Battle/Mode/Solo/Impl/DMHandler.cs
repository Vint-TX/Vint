using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Mode.Solo.Impl;

public class DMHandler(
    Round round,
    Func<IEntity> entityFactory
) : SoloHandler(round) {
    public override IEntity Entity { get; } = entityFactory();
    protected override IList<SpawnPoint> SpawnPoints { get; } = round.Properties.MapInfo.SpawnPoints.Deathmatch;

    public override int CalculateReputationDelta(Tanker tanker) {
        List<Tanker> tankers = Round.Tankers
            .OrderByDescending(t => t.Tank.Result.ScoreWithoutPremium)
            .ToList();

        if (tankers.Count < 2)
            return 0;

        Database.Models.Player player = tanker.Connection.Player;
        int index = tankers.IndexOf(tanker);

        return MathUtils.Map(index, 0, tankers.Count - 1, player.MaxReputationDelta, player.MinReputationDelta);
    }
}
