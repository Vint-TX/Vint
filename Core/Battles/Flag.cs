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
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles;

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
    public HashSet<BattlePlayer> Assistants { get; } = [];

    public void Capture(BattlePlayer carrier) { // todo modules
        if (StateManager.CurrentState is not OnPedestal) return;

        StateManager.SetState(new Captured(StateManager, carrier.Tank!.Tank));

        Carrier = carrier;
        Assistants.Add(carrier);
    }

    public void Drop(bool isUserAction) { // todo height maps (or server physics)
        if (StateManager.CurrentState is not Captured) return;

        LastCarrier = Carrier;
        UnfrozeForLastCarrierTime = DateTimeOffset.UtcNow.AddSeconds(3);

        StateManager.SetState(new OnGround(StateManager, isUserAction));
        Carrier = null;
    }

    public void Pickup(BattlePlayer carrier) { // todo modules
        if (StateManager.CurrentState is not OnGround ||
            LastCarrier == carrier &&
            UnfrozeForLastCarrierTime > DateTimeOffset.UtcNow) return;

        StateManager.SetState(new Captured(StateManager, carrier.Tank!.Tank));
        Carrier = carrier;
        Assistants.Add(carrier);
    }

    public void Return(BattlePlayer? returner = null) {
        if (StateManager.CurrentState is not OnGround) return;

        StateManager.SetState(new OnPedestal(StateManager));

        if (returner != null) {
            Entity.AddComponent(new TankGroupComponent(returner.Tank!.Tank));
            returner.PlayerConnection.Send(new VisualScoreFlagReturnEvent(0), returner.BattleUser);
        }

        foreach (BattlePlayer battlePlayer in Battle.Players.ToList())
            battlePlayer.PlayerConnection.Send(new FlagReturnEvent(), Entity);

        if (returner != null)
            Entity.RemoveComponent<TankGroupComponent>();

        Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = PedestalPosition);
        Entity.RemoveComponent<FlagGroundedStateComponent>();
        Entity.AddComponent(new FlagHomeStateComponent());

        LastCarrier = null;
        Refresh();

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
            foreach (BattlePlayer player in Battle.Players.ToList())
                player.PlayerConnection.Send(new FlagDeliveryEvent(), Entity);
        } else {
            foreach (BattlePlayer player in Battle.Players.ToList())
                player.PlayerConnection.Send(new FlagNotCountedDeliveryEvent(), Battle.Entity);
        }

        Entity.RemoveComponent<TankGroupComponent>();
        Entity.ChangeComponent<FlagPositionComponent>(component => component.Position = PedestalPosition);

        Refresh();
        Carrier = null;
        LastCarrier = null;
        Assistants.Clear();

        using DbConnection db = new();
        db.Statistics
            .Where(stats => stats.PlayerId == battlePlayer.PlayerConnection.Player.Id)
            .Set(stats => stats.FlagsDelivered, stats => stats.FlagsDelivered + 1)
            .Update();
    }

    public void CarrierDied() {
        Drop(false);

        if (PhysicsUtils.IsOutsideMap(Battle.MapInfo.PuntativeGeoms, Position, Vector3.Zero, Battle.Properties.KillZoneEnabled))
            Return();
    }

    void Refresh() {
        foreach (IPlayerConnection playerConnection in Entity.SharedPlayers.ToList()) {
            playerConnection.Unshare(Entity);
            playerConnection.Share(Entity);
        }
    }
}