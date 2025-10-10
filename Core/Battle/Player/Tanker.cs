using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Rounds.Components;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Battle.Player;

public abstract class Tanker(
    Round round,
    IPlayerConnection connection
) : BattlePlayer(round, connection) {
    public abstract BattleTank Tank { get; }
    public abstract IEntity? Team { get; }
    public abstract TeamColor TeamColor { get; }
    public abstract TeamBattleResult TeamResult { get; }

    public abstract bool Reported { get; set; }

    public abstract float ScoreMultiplier { get; }

    public override async Task Init() {
        await base.Init();

        await Tank.Init();
        await Round.Players.Share(Tank.Entities);
    }

    public override async Task DeInit() {
        await Round.Players.Unshare(Tank.Entities);
        await Tank.DeInit();

        await base.DeInit();
    }

    public abstract int GetScoreWithBonus(int score);

    public int GetBattleUserScoreWithBonus() {
        int score = Tank.Entities.RoundUser.GetComponent<RoundUserStatisticsComponent>().ScoreWithoutBonuses;
        return GetScoreWithBonus(score);
    }
}
