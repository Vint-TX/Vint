using System.Numerics;
using Serilog;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Server.Battle;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Bonus;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Bonus;

public abstract class BonusBox {
    protected BonusBox(Round round, Vector3 regionPosition, bool hasParachute) {
        Round = round;
        RegionPosition = regionPosition;
        if (!hasParachute) SpawnHeight = 0;

        StateManager = new BonusStateManager(this);
        Logger = Log.Logger.ForType(GetType());
    }

    protected ILogger Logger { get; }
    protected bool CanTake { get; private set; }

    public abstract BonusType Type { get; }
    public abstract IEntity? Entity { get; protected set; }
    public abstract Lazy<IEntity> RegionEntity { get; protected set; }
    public abstract BonusConfigComponent ConfigComponent { get; }

    public Round Round { get; }
    public BonusStateManager StateManager { get; }
    public float SpawnHeight { get; } = 30; // todo map dependent?

    public Vector3 RegionPosition { get; }
    public Vector3 SpawnPosition => RegionPosition with { Y = RegionPosition.Y + SpawnHeight };

    public virtual async Task Init() =>
        await StateManager.Init();

    public virtual async Task Take(BattleTank battleTank) {
        if (Entity == null) {
            Logger.Error("{Connection} wanted to take nonexistent bonus", battleTank.Tanker.Connection);
            return;
        }

        ICollection<BattlePlayer> players = Round.Players;
        await players.Send(new BonusTakenEvent(), Entity);
        await players.Unshare(Entity);

        battleTank.Statistics.BonusesTaken++;
        Entity = null;
        CanTake = true;
    }

    public abstract Task Spawn();

    public abstract bool CanBeDropped(bool force);

    public virtual Task Drop() => Spawn();

    public virtual Task Tick(TimeSpan deltaTime) => StateManager.Tick(deltaTime);
}
