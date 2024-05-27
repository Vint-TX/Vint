using System.Numerics;
using LinqToDB;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Player;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Battle.Flag;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle.Flag;
using Vint.Core.ECS.Events.Battle.Score.Visual;
using Vint.Core.ECS.Templates.Battle.Flag;
using Vint.Core.Physics;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Flags;

public class Flag {
    public Flag(Battle battle, IEntity team, TeamColor teamColor, Vector3 pedestalPosition) {
        Battle = battle;
        TeamEntity = team;
        TeamColor = teamColor;
        PedestalPosition = pedestalPosition;
        StateManager = new FlagStateManager(this);

        PedestalEntity = new PedestalTemplate().Create(pedestalPosition, TeamEntity, Battle.Entity);
        Entity = new FlagTemplate().Create(pedestalPosition, TeamEntity, Battle.Entity);
    }

    public FlagStateManager StateManager { get; }

    public IEntity TeamEntity { get; }
    public IEntity PedestalEntity { get; }
    public IEntity Entity { get; }

    public Battle Battle { get; private set; }
    public TeamColor TeamColor { get; private set; }
    public Vector3 PedestalPosition { get; }
    public Vector3 Position => Entity.GetComponent<FlagPositionComponent>().Position;

    public BattlePlayer? Carrier { get; private set; }
    public BattlePlayer? LastCarrier { get; set; }
    public DateTimeOffset UnfrozeForLastCarrierTime { get; private set; }
    public HashSet<FlagAssist> Assists { get; } = [];

    public async Task Capture(BattlePlayer carrier) {
        if (StateManager.CurrentState is not OnPedestal) return;

        await StateManager.SetState(new Captured(StateManager, carrier.Tank!.Tank));

        Carrier = carrier;
        Assists.Add(new FlagAssist(carrier.Tank!, Position));

        foreach (IFlagModule flagModule in carrier.Tank.Modules.OfType<IFlagModule>())
            await flagModule.OnFlagAction(FlagAction.Capture);
    }

    public async Task Drop(bool isUserAction) {
        if (StateManager.CurrentState is not Captured) return;

        LastCarrier = Carrier;
        Carrier = null;

        Vector3 newPosition;
        Vector3 tankPosition = LastCarrier!.Tank!.Position;
        RayHitHandler hitHandler = new();
        Battle.Simulation?.RayCast(tankPosition, -Vector3.UnitY, 655.36f, ref hitHandler);

        if (Battle.Simulation == null) newPosition = tankPosition - Vector3.UnitY;
        else if (!hitHandler.ClosestHit.HasValue) newPosition = Vector3.UnitY * 1000;
        else newPosition = hitHandler.ClosestHit.Value;

        await StateManager.SetState(new OnGround(StateManager, newPosition, isUserAction));

        foreach (IFlagModule flagModule in LastCarrier.Tank.Modules.OfType<IFlagModule>())
            await flagModule.OnFlagAction(FlagAction.Drop);

        if (PhysicsUtils.IsOutsideMap(Battle.MapInfo.PuntativeGeoms, newPosition, Vector3.Zero, Battle.Properties.KillZoneEnabled)) {
            await Return();
            return;
        }

        UnfrozeForLastCarrierTime = DateTimeOffset.UtcNow.AddSeconds(3);

        FlagAssist assist = Assists.First(assist => assist.Tank == LastCarrier?.Tank);
        assist.TraveledDistance += Vector3.Distance(assist.LastPickupPoint, Position);
    }

    public async Task Pickup(BattlePlayer carrier) {
        if (StateManager.CurrentState is not OnGround ||
            LastCarrier == carrier &&
            UnfrozeForLastCarrierTime > DateTimeOffset.UtcNow) return;

        await StateManager.SetState(new Captured(StateManager, carrier.Tank!.Tank));
        Carrier = carrier;

        FlagAssist assist = Assists.FirstOrDefault(assist => assist.Tank == carrier.Tank!, new FlagAssist(carrier.Tank, Position));

        assist.LastPickupPoint = Position;
        Assists.Add(assist);

        foreach (IFlagModule flagModule in carrier.Tank.Modules.OfType<IFlagModule>())
            await flagModule.OnFlagAction(FlagAction.Capture);
    }

    public async Task Return(BattlePlayer? returner = null) {
        if (StateManager.CurrentState is not OnGround ||
            Battle.ModeHandler is not CTFHandler ctf) return;

        await StateManager.SetState(new OnPedestal(StateManager));

        if (returner != null) {
            await Entity.AddGroupComponent<TankGroupComponent>(returner.Tank!.Tank);

            Vector3 carrierPedestal = ctf.Flags.First(flag => flag.TeamColor == LastCarrier?.TeamColor).PedestalPosition;
            Vector3 returnPosition = Position;

            float maxDistance = Vector3.Distance(PedestalPosition, carrierPedestal);
            float distanceToEnemyPedestal = Vector3.Distance(returnPosition, carrierPedestal);
            float remainingDistance = float.Min(distanceToEnemyPedestal, maxDistance);
            int score = Convert.ToInt32(Math.Round(MathUtils.Map(remainingDistance, maxDistance, 0, 2, 50)));
            int scoreWithBonus = returner.GetScoreWithBonus(score);

            BattleTank returnerTank = returner.Tank!;

            await returnerTank.AddScore(score);
            await returnerTank.CommitStatistics();

            await returner.PlayerConnection.Send(new VisualScoreFlagReturnEvent(scoreWithBonus), returner.BattleUser);
            returnerTank.Statistics.FlagReturns += 1;
        }

        foreach (BattlePlayer battlePlayer in Battle.Players)
            await battlePlayer.PlayerConnection.Send(new FlagReturnEvent(), Entity);

        if (returner != null)
            await Entity.RemoveComponent<TankGroupComponent>();

        await Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = PedestalPosition);
        await Entity.RemoveComponent<FlagGroundedStateComponent>();
        await Entity.AddComponent<FlagHomeStateComponent>();

        await Refresh();
        Assists.Clear();
        LastCarrier = null;

        if (returner != null) {
            foreach (IFlagModule flagModule in returner.Tank!.Modules.OfType<IFlagModule>())
                await flagModule.OnFlagAction(FlagAction.Return);
        }

        if (returner == null) return;

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == returner.PlayerConnection.Player.Id)
            .Set(stats => stats.FlagsReturned, stats => stats.FlagsReturned + 1)
            .UpdateAsync();
    }

    public async Task Deliver(BattlePlayer battlePlayer) {
        if (StateManager.CurrentState is not Captured ||
            Battle.ModeHandler is not CTFHandler ctf) return;

        await StateManager.SetState(new OnPedestal(StateManager));
        await Entity.AddComponent<FlagHomeStateComponent>();

        if (ctf.RedPlayers.Any(player => player.InBattleAsTank) &&
            ctf.BluePlayers.Any(player => player.InBattleAsTank)) {
            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                await player.PlayerConnection.Send(new FlagDeliveryEvent(), Entity);

            await Battle.ModeHandler.UpdateScore(battlePlayer.Team, 1);

            BattleTank tank = battlePlayer.Tank!;
            FlagAssist carrierAssist = Assists.First(assist => assist.Tank == tank);
            Vector3 carrierPedestal = ctf.Flags.First(flag => flag.TeamColor == battlePlayer.TeamColor).PedestalPosition;

            float maxDistance = Vector3.Distance(PedestalPosition, carrierPedestal);
            float carrierDistance = carrierAssist.TraveledDistance + Vector3.Distance(carrierAssist.LastPickupPoint, carrierPedestal);
            float traveledDistance = float.Min(carrierDistance, maxDistance);
            int score = Convert.ToInt32(Math.Round(MathUtils.Map(traveledDistance, 0, maxDistance, 10, 75)));
            int scoreWithBonus = battlePlayer.GetScoreWithBonus(score);

            tank.Statistics.Flags += 1;
            await tank.AddScore(score);
            await tank.CommitStatistics();

            await battlePlayer.PlayerConnection.Send(new VisualScoreFlagDeliverEvent(scoreWithBonus), battlePlayer.BattleUser);

            foreach (FlagAssist assist in Assists.Where(assist => assist.Tank != tank)) {
                BattleTank assistant = assist.Tank;

                float distance = float.Min(assist.TraveledDistance, maxDistance);
                int assistScore = Convert.ToInt32(Math.Round(MathUtils.Map(distance, 0, maxDistance, 2, 75 - score)));
                int assistScoreWithBonus = assistant.BattlePlayer.GetScoreWithBonus(assistScore);

                assistant.Statistics.FlagAssists += 1;
                await assistant.AddScore(assistScore);
                await assistant.CommitStatistics();

                await assistant.BattlePlayer.PlayerConnection.Send(new VisualScoreFlagDeliverEvent(assistScoreWithBonus), assistant.BattleUser);
            }
        } else
            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                await player.PlayerConnection.Send(new FlagNotCountedDeliveryEvent(), Entity, Battle.Entity);

        await Entity.RemoveComponent<TankGroupComponent>();
        await Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = PedestalPosition);

        await Refresh();
        Assists.Clear();
        Carrier = null;
        LastCarrier = null;

        foreach (IFlagModule flagModule in battlePlayer.Tank!.Modules.OfType<IFlagModule>())
            await flagModule.OnFlagAction(FlagAction.Deliver);

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == battlePlayer.PlayerConnection.Player.Id)
            .Set(stats => stats.FlagsDelivered, stats => stats.FlagsDelivered + 1)
            .UpdateAsync();
    }

    async Task Refresh() {
        foreach (IPlayerConnection playerConnection in Entity.SharedPlayers) {
            await playerConnection.Unshare(Entity);
            await playerConnection.Share(Entity);
        }
    }
}
