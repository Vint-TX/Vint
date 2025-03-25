using Vint.Core.Battle.Player.User.Templates;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Results.Events;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.Quests;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Player;

public class Spectator(
    IPlayerConnection connection,
    Round round
) : BattlePlayer(round, connection) {
    public override IEntity BattleUser { get; } = new BattleUserTemplate().CreateAsSpectator(connection.UserContainer.Entity, round.Entity);

    public override async Task Init() {
        await base.Init();

        foreach (Tanker tanker in Round.Tankers)
            await tanker.Connection.UserContainer.ShareTo(Connection);

        Connection.Spectator = this;
    }

    public override async Task DeInit() {
        Connection.Spectator = null;

        foreach (Tanker tanker in Round.Tankers)
            await tanker.Connection.UserContainer.UnshareFrom(Connection);

        await Connection.Unshare(BattleUser);
        await base.DeInit();
    }

    public override async Task OnRoundEnded(bool hasEnemies, QuestManager questManager) {
        BattleResultForClient result = BattleResultForClient.CreateForSpectator(Round);
        await Connection.Send(new BattleResultForClientEvent(result), Connection.UserContainer.Entity);
    }

    public override Task Tick(TimeSpan deltaTime) => Task.CompletedTask;
}
