using System.Numerics;
using LinqToDB;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
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

    public void Capture(BattlePlayer carrier) { // todo modules
        if (StateManager.CurrentState is not OnPedestal) return;

        StateManager.SetState(new Captured(StateManager, carrier.Tank!.Tank));

        Carrier = carrier;
        Assists.Add(new FlagAssist(carrier.Tank!, Position));
    }

    public void Drop(bool isUserAction) {
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

        StateManager.SetState(new OnGround(StateManager, newPosition, isUserAction));

        if (PhysicsUtils.IsOutsideMap(Battle.MapInfo.PuntativeGeoms, newPosition, Vector3.Zero, Battle.Properties.KillZoneEnabled)) {
            Return();
            return;
        }

        UnfrozeForLastCarrierTime = DateTimeOffset.UtcNow.AddSeconds(3);

        FlagAssist assist = Assists.SingleOrDefault(assist => assist.Player == LastCarrier?.Tank);
        assist.TraveledDistance += Vector3.Distance(assist.LastPickupPoint, Position);
    }

    public void Pickup(BattlePlayer carrier) { // todo modules
        if (StateManager.CurrentState is not OnGround ||
            LastCarrier == carrier &&
            UnfrozeForLastCarrierTime > DateTimeOffset.UtcNow) return;

        StateManager.SetState(new Captured(StateManager, carrier.Tank!.Tank));
        Carrier = carrier;

        FlagAssist assist = Assists.SingleOrDefault(assist => assist.Player == carrier.Tank!, new FlagAssist(carrier.Tank, Position));
        assist.LastPickupPoint = Position;

        Assists.Add(assist);
    }

    public void Return(BattlePlayer? returner = null) {
        if (StateManager.CurrentState is not OnGround ||
            Battle.ModeHandler is not CTFHandler ctf) return;

        StateManager.SetState(new OnPedestal(StateManager));

        if (returner != null) {
            Entity.AddComponent(new TankGroupComponent(returner.Tank!.Tank));

            Vector3 carrierPedestal = ctf.Flags[LastCarrier!.TeamColor].PedestalPosition;
            Vector3 returnPosition = Position;

            float maxDistance = Vector3.Distance(PedestalPosition, carrierPedestal);
            float traveledDistance = float.Min(Vector3.Distance(PedestalPosition, returnPosition), maxDistance);
            int score = Convert.ToInt32(Math.Round(MathUtils.Map(traveledDistance, 0, maxDistance, 5, 40)));
            int scoreWithBonus = returner.GetScoreWithBonus(score);

            returner.Tank!.UpdateStatistics(0, 0, 0, score);
            returner.PlayerConnection.Send(new VisualScoreFlagReturnEvent(scoreWithBonus), returner.BattleUser);
            returner.Tank!.UserResult.FlagReturns += 1;
        }

        foreach (BattlePlayer battlePlayer in Battle.Players)
            battlePlayer.PlayerConnection.Send(new FlagReturnEvent(), Entity);

        if (returner != null)
            Entity.RemoveComponent<TankGroupComponent>();

        Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = PedestalPosition);
        Entity.RemoveComponent<FlagGroundedStateComponent>();
        Entity.AddComponent(new FlagHomeStateComponent());

        Refresh();
        Assists.Clear();
        LastCarrier = null;

        if (returner == null) return;

        using DbConnection db = new();
        db.Statistics
            .Where(stats => stats.PlayerId == returner.PlayerConnection.Player.Id)
            .Set(stats => stats.FlagsReturned, stats => stats.FlagsReturned + 1)
            .Update();
    }

    public void Deliver(BattlePlayer battlePlayer) {
        if (StateManager.CurrentState is not Captured ||
            Battle.ModeHandler is not CTFHandler ctf) return;

        StateManager.SetState(new OnPedestal(StateManager));
        Entity.AddComponent(new FlagHomeStateComponent());

        if (ctf.RedPlayers.Any(player => player.InBattleAsTank) &&
            ctf.BluePlayers.Any(player => player.InBattleAsTank)) {
            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                player.PlayerConnection.Send(new FlagDeliveryEvent(), Entity);

            Battle.ModeHandler.UpdateScore(battlePlayer.Team, 1);

            Vector3 carrierPedestal = ctf.Flags[battlePlayer.TeamColor].PedestalPosition;
            Vector3 pickupPosition = Position;

            float maxDistance = Vector3.Distance(PedestalPosition, carrierPedestal);
            float traveledDistance = float.Min(Vector3.Distance(pickupPosition, carrierPedestal), maxDistance);
            int score = Convert.ToInt32(Math.Round(MathUtils.Map(traveledDistance, 0, maxDistance, 10, 75)));
            int scoreWithBonus = battlePlayer.GetScoreWithBonus(score);

            battlePlayer.Tank!.UserResult.Flags += 1;
            battlePlayer.Tank!.UpdateStatistics(0, 0, 0, score);
            battlePlayer.PlayerConnection.Send(new VisualScoreFlagDeliverEvent(scoreWithBonus), battlePlayer.BattleUser);

            foreach ((BattleTank? assistant, _, float dist) in Assists.Where(assist => assist.Player != battlePlayer.Tank)) {
                float distance = float.Min(dist, maxDistance);
                int assistScore = Convert.ToInt32(Math.Round(MathUtils.Map(distance, 0, maxDistance, 1, 30)));
                int assistScoreWithBonus = assistant.BattlePlayer.GetScoreWithBonus(assistScore);

                assistant.UserResult.FlagAssists += 1;
                assistant.UpdateStatistics(0, 0, 0, assistScore);
                assistant.BattlePlayer.PlayerConnection.Send(new VisualScoreFlagDeliverEvent(assistScoreWithBonus), assistant.BattleUser);
            }
        } else
            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                player.PlayerConnection.Send(new FlagNotCountedDeliveryEvent(), Battle.Entity);

        Entity.RemoveComponent<TankGroupComponent>();
        Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = PedestalPosition);

        Refresh();
        Assists.Clear();
        Carrier = null;
        LastCarrier = null;

        using DbConnection db = new();
        db.Statistics
            .Where(stats => stats.PlayerId == battlePlayer.PlayerConnection.Player.Id)
            .Set(stats => stats.FlagsDelivered, stats => stats.FlagsDelivered + 1)
            .Update();
    }

    void Refresh() {
        foreach (IPlayerConnection playerConnection in Entity.SharedPlayers) {
            playerConnection.Unshare(Entity);
            playerConnection.Share(Entity);
        }
    }
}