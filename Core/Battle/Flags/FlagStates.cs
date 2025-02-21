using System.Numerics;
using ConcurrentCollections;
using LinqToDB;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Database;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle.Flag;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Flag;
using Vint.Core.ECS.Events.Battle.Score.Visual;
using Vint.Core.Physics;
using Vint.Core.Server.Game;
using Vint.Core.StateMachine;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Flags;

public class FlagStateManager(
    Flag flag
) : StateManager<FlagState> {
    public Flag Flag { get; } = flag;
    public CTFHandler ModeHandler { get; private set; } = null!;

    public ConcurrentHashSet<FlagAssist> Assists { get; } = [];

    public override async Task Init() {
        await InitState(new OnPedestal(this));
        ModeHandler = (CTFHandler)Flag.Round.ModeHandler;
    }
}

public abstract class FlagState(
    FlagStateManager stateManager
) : State {
    public override FlagStateManager StateManager { get; } = stateManager;
    protected ICollection<FlagAssist> Assists => StateManager.Assists;
    protected CTFHandler ModeHandler => StateManager.ModeHandler;
    protected Flag Flag => StateManager.Flag;
    protected Round Round => Flag.Round;
    protected IEntity Entity => Flag.Entity;

    protected async Task Refresh() {
        foreach (IPlayerConnection connection in Entity.SharedPlayers) {
            await connection.Unshare(Entity);
            await connection.Share(Entity);
        }
    }
}

public abstract class FlagState<TComponent>(
    FlagStateManager stateManager
) : FlagState(stateManager) where TComponent : class, IComponent, new() {
    TComponent StateComponent { get; } = new();

    public override async Task Start() {
        await Entity.AddComponent(StateComponent);
        await base.Start();
    }

    public override async Task Finish() {
        await Entity.RemoveComponent<TComponent>();
        await base.Finish();
    }
}

public class OnPedestal(
    FlagStateManager stateManager
) : FlagState<FlagHomeStateComponent>(stateManager) {
    public async Task Capture(Tanker carrier) =>
        await StateManager.SetState(new Captured(StateManager, carrier));

    public override async Task Start() {
        await Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = Flag.PedestalPosition);
        await base.Start();
    }
}

public class Captured(
    FlagStateManager stateManager,
    Tanker carrier
) : FlagState(stateManager) {
    public Tanker Carrier { get; } = carrier;

    public async Task Deliver() {
        if (!ModeHandler.RedTeam.Players.Any() || !ModeHandler.BlueTeam.Players.Any())
            await Round.Players.Send(new FlagNotCountedDeliveryEvent(), Entity, ModeHandler.Entity);
        else {
            await Round.Players.Send(new FlagDeliveryEvent(), Entity);
            await ModeHandler.UpdateScore(Carrier.TeamColor, 1);

            BattleTank tank = Carrier.Tank;
            FlagAssist carrierAssist = Assists.First(assist => assist.Tank == tank);
            Vector3 carrierPedestal = ModeHandler.Flags[Carrier.TeamColor].PedestalPosition;

            float maxDistance = Vector3.Distance(Flag.PedestalPosition, carrierPedestal);
            float carrierDistance = carrierAssist.TraveledDistance + Vector3.Distance(carrierAssist.LastPickupPoint, carrierPedestal);
            float traveledDistance = float.Min(carrierDistance, maxDistance);
            int score = Convert.ToInt32(Math.Round(MathUtils.Map(traveledDistance, 0, maxDistance, 10, 75)));
            int scoreWithBonus = Carrier.GetScoreWithBonus(score);

            tank.Statistics.Flags += 1;
            await tank.AddScore(score);
            await tank.CommitStatistics();

            await Carrier.Send(new VisualScoreFlagDeliverEvent(scoreWithBonus), Carrier.BattleUser);

            foreach (FlagAssist assist in Assists.Where(assist => assist.Tank != tank)) {
                BattleTank assistant = assist.Tank;

                float distance = float.Min(assist.TraveledDistance, maxDistance);
                int assistScore = Convert.ToInt32(Math.Round(MathUtils.Map(distance, 0, maxDistance, 2, 75 - score)));
                int assistScoreWithBonus = assistant.Tanker.GetScoreWithBonus(assistScore);

                assistant.Statistics.FlagAssists += 1;
                await assistant.AddScore(assistScore);
                await assistant.CommitStatistics();

                await assistant.Tanker.Connection.Send(new VisualScoreFlagDeliverEvent(assistScoreWithBonus), assistant.Entities.BattleUser);
            }
        }

        await StateManager.SetState(new OnPedestal(StateManager));

        await Refresh();
        Assists.Clear();

        foreach (IFlagModule flagModule in Carrier.Tank.Modules.OfType<IFlagModule>())
            await flagModule.OnFlagAction(FlagAction.Deliver);

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == Carrier.Connection.Player.Id)
            .Set(stats => stats.FlagsDelivered, stats => stats.FlagsDelivered + 1)
            .UpdateAsync();
    }

    public async Task Drop(bool isUserAction) {
        RayClosestHitHandler hitHandler = new();
        Round.Simulation.RayCast(Carrier.Tank.Position, -Vector3.UnitY, 655.36f, ref hitHandler);
        Vector3 newPosition = hitHandler.ClosestHit ?? Vector3.NaN;

        FlagAssist assist = Assists.First(assist => assist.Tank == Carrier.Tank);
        assist.TraveledDistance += Vector3.Distance(assist.LastPickupPoint, newPosition);

        await Round.Players.Send(new FlagDropEvent(isUserAction), Entity);
        await StateManager.SetState(new OnGround(StateManager, Carrier, newPosition));
    }

    public override async Task Start() {
        await Entity.AddGroupComponent<TankGroupComponent>(Carrier.Tank.Entities.Tank);
        await Round.Players.Send(new FlagPickupEvent(), Entity);

        await base.Start();
    }

    public override async Task Started() {
        Vector3 position = Flag.Position;
        FlagAssist assist = Assists.FirstOrDefault(assist => assist.Tank == Carrier.Tank, new FlagAssist(Carrier.Tank, position));

        assist.LastPickupPoint = position;
        Assists.Add(assist);

        foreach (IFlagModule flagModule in Carrier.Tank.Modules.OfType<IFlagModule>())
            await flagModule.OnFlagAction(FlagAction.Capture);

        await base.Started();
    }

    public override async Task Finish() {
        await Entity.RemoveComponent<TankGroupComponent>();
        await base.Finish();
    }
}

public class OnGround : FlagState<FlagGroundedStateComponent> {
    public OnGround(FlagStateManager stateManager, Tanker carrier, Vector3 position) : base(stateManager) {
        Position = position;
        LastCarrier = carrier;

        UnfrozeForLastCarrierTime = DateTimeOffset.UtcNow + Flag.EnemyFlagActionInterval;
        ReturnAtTime = DateTimeOffset.UtcNow.AddMinutes(1);
    }

    public Tanker LastCarrier { get; }

    DateTimeOffset UnfrozeForLastCarrierTime { get; }
    DateTimeOffset ReturnAtTime { get; }

    Vector3 Position { get; }

    public async Task Pickup(Tanker carrier) {
        if (LastCarrier == carrier && UnfrozeForLastCarrierTime > DateTimeOffset.UtcNow)
            return;

        await StateManager.SetState(new Captured(StateManager, carrier));
    }

    public async Task Return(Tanker? returner = null) {
        if (returner != null) {
            BattleTank returnerTank = returner.Tank;
            Vector3 ownPedestal = Flag.PedestalPosition;
            Vector3 enemyPedestal = ModeHandler.Flags[LastCarrier.TeamColor].PedestalPosition;

            float maxDistance = Vector3.Distance(ownPedestal, enemyPedestal);
            float distanceToEnemyPedestal = Vector3.Distance(Position, enemyPedestal);
            float remainingDistance = float.Min(distanceToEnemyPedestal, maxDistance);
            int score = (int)Math.Ceiling(MathUtils.Map(remainingDistance, maxDistance, 0, 2, 50));
            int scoreWithBonus = returner.GetScoreWithBonus(score);

            await returnerTank.AddScore(score);
            await returnerTank.CommitStatistics();

            await returner.Send(new VisualScoreFlagReturnEvent(scoreWithBonus), returner.BattleUser);
            returnerTank.Statistics.FlagReturns += 1;

            await Entity.AddGroupComponent<TankGroupComponent>(returnerTank.Entities.Tank);
        }

        await Round.Players.Send(new FlagReturnEvent(), Entity);
        await StateManager.SetState(new OnPedestal(StateManager));

        if (returner != null)
            await Entity.RemoveComponent<TankGroupComponent>();

        await Refresh();
        Assists.Clear();

        if (returner != null) {
            foreach (IFlagModule flagModule in returner.Tank.Modules.OfType<IFlagModule>())
                await flagModule.OnFlagAction(FlagAction.Return);

            await using DbConnection db = new();
            await db.Statistics
                .Where(stats => stats.PlayerId == returner.Connection.Player.Id)
                .Set(stats => stats.FlagsReturned, stats => stats.FlagsReturned + 1)
                .UpdateAsync();
        }
    }

    public override async Task Start() {
        await Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = Position);
        await base.Start();
    }

    public override async Task Started() {
        foreach (IFlagModule flagModule in LastCarrier.Tank.Modules.OfType<IFlagModule>())
            await flagModule.OnFlagAction(FlagAction.Drop);

        if (PhysicsUtils.IsOutsideMap(Round.Properties.MapInfo.PuntativeGeoms, Position, Vector3.Zero, Round.Properties.KillZoneEnabled)) {
            await Return();
            return;
        }

        await base.Started();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (DateTimeOffset.UtcNow < ReturnAtTime)
            return;

        await Return();
    }
}
