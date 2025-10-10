using System.Diagnostics.CodeAnalysis;
using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Autopilot.Events;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Player.User.Components;
using Vint.Core.Battle.Player.User.Templates;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.State;
using Vint.Core.ECS.Entities;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Player;

public sealed class BotTanker : Tanker {
    public BotTanker(Round round, IPlayerConnection connection, IEntity? team) : base(round, connection) {
        BotId = Id.GetHashCode();
        Team = team;
        TeamColor = Team?.GetComponent<TeamColorComponent>().TeamColor ?? TeamColor.None;
        BattleUser = new BattleUserTemplate().CreateAsTank(connection.UserContainer.Entity, round.Entity, team);
        Tank = new BattleTank(this);
    }

    static TimeSpan FindControllerTimeout => TimeSpan.FromSeconds(30);
    static TimeSpan WaitForAcceptTimeout => TimeSpan.FromSeconds(2);

    public int BotId { get; }
    public HumanTanker? Controller { get; private set; }

    public override IEntity BattleUser { get; }
    public override BattleTank Tank { get; }
    public override IEntity? Team { get; }
    public override TeamColor TeamColor { get; }
    public override TeamBattleResult TeamResult => Tank.Result.TeamResult;

    public override bool Reported { get; set; }

    public override float ScoreMultiplier => 1;

    List<HumanTanker> RequestedControllers { get; } = [];
    HumanTanker? LastRequestedController => RequestedControllers.LastOrDefault();

    DateTimeOffset? FindControllerEndTime { get; set; }
    DateTimeOffset? ControlRequestSentEndTime { get; set; }

    DateTimeOffset ParamsUpdateTime { get; set; } = DateTimeOffset.UtcNow;

    public override async Task Init() {
        await base.Init();

        IEntity tank = Tank.Entities.Tank;
        await tank.AddComponent(new TankAutopilotComponent(BotId));
        await tank.AddComponent<AutopilotWeaponControllerComponent>();
        await tank.AddComponent<AutopilotMovementControllerComponent>();

        await StartFindController();
    }

    public override Task OnRoundEnded(bool hasEnemies, QuestManager questManager) => Task.CompletedTask;

    public override int GetScoreWithBonus(int score) => score;

    public override async Task Tick(TimeSpan deltaTime, CancellationToken cancellationToken) {
        if (FindControllerEndTime.HasValue && DateTimeOffset.UtcNow >= FindControllerEndTime) {
            await Round.RemoveTanker(this);
            return;
        }

        if (ControlRequestSentEndTime.HasValue && DateTimeOffset.UtcNow >= ControlRequestSentEndTime)
            await SendControllerRequest();

        if (DateTimeOffset.UtcNow >= ParamsUpdateTime) {
            await Tank.Entities.Tank.ChangeComponent<AutopilotMovementControllerComponent>(component => {
                component.Moving = true;
                component.MoveToTarget = true;
                component.Target = GetRandomTarget();
            });

            await Tank.Entities.Tank.ChangeComponent<AutopilotWeaponControllerComponent>(component => {
                component.Attack = true;
                component.Accuracy = 1f;
            });

            ParamsUpdateTime = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(Random.Shared.Next(7, 20));
        }

        await Tank.Tick(deltaTime, cancellationToken);
        return;

        IEntity? GetRandomTarget() {
            List<Tanker> tankers;

            if (Round.ModeHandler is not TeamHandler teamHandler)
                tankers = Round.Tankers.ToList();
            else {
                TeamData enemyTeam = TeamColor == TeamColor.Red
                    ? teamHandler.BlueTeam
                    : teamHandler.RedTeam;

                tankers = enemyTeam.Players.ToList();
            }

            return tankers.Count == 0
                ? null
                : tankers.RandomElement().Tank.Entities.Tank;
        }
    }

    public async Task OnHumanLeave(HumanTanker human) {
        if (Controller != human) return;

        if (Round.StateManager.CurrentState is Ended)
            await Round.RemoveTanker(this);
        else await StartFindController();
    }

    public async Task OnControllerFound(HumanTanker controller) {
        if (Controller != null || LastRequestedController != controller)
            return;

        await SaveController(controller);
    }

    [MemberNotNull(nameof(Controller))]
    async Task SaveController(HumanTanker controller) {
        ClearFields();
        Controller = controller;
        Controller.ControlledBots.TryAdd(BotId, this);

        await Tank.Entities.Tank.ChangeComponent<TankAutopilotComponent>(c => c.Session = Controller.Connection.ClientSession);
        await Round.HumanTankers.Send<ChangeAutopilotControllerEvent>(Tank.Entities.Tank);

        // ReSharper disable once InvertIf
        if (!BattleUser.HasComponent<UserReadyToBattleComponent>()) {
            await BattleUser.AddComponent<UserReadyToBattleComponent>();
            await Tank.StateManager.SetState(new Spawn(Tank.StateManager));
        }
    }

    async Task ResetController() {
        if (Controller != null) {
            Controller.ControlledBots.TryRemove(BotId, out _);
            Controller = null;

            IEntity tank = Tank.Entities.Tank;
            await tank.ChangeComponent<TankAutopilotComponent>(component => component.Session = null);
            await Round.HumanTankers.Send<ChangeAutopilotControllerEvent>(tank);
        }

        ClearFields();
    }

    async Task StartFindController() {
        if (FindControllerEndTime.HasValue)
            return;

        await ResetController();

        FindControllerEndTime = DateTimeOffset.UtcNow + FindControllerTimeout;
        await SendControllerRequest();
    }

    async Task SendControllerRequest() {
        ControlRequestSentEndTime = DateTimeOffset.UtcNow + WaitForAcceptTimeout;

        List<HumanTanker> candidates = Round.HumanTankers
            .Where(human => human.BattleUser.HasComponent<UserReadyToBattleComponent>())
            .Except(RequestedControllers)
            .ToList();

        if (candidates.Count == 0) return;

        HumanTanker candidate = candidates.MinBy(human => human.Connection.Ping)!;
        IEntity tank = Tank.Entities.Tank;

        await candidate.Send(new RequestAutopilotControllerEvent(), tank);
        RequestedControllers.Add(candidate);
    }

    void ClearFields() {
        FindControllerEndTime = null;
        ControlRequestSentEndTime = null;
        RequestedControllers.Clear();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            Tank.Dispose();
        }
    }
}
