using System.Numerics;
using Serilog;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Bonus;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Bonus;

public abstract class BonusBox {
    protected BonusBox(Battle battle, Vector3 regionPosition, bool hasParachute) {
        Battle = battle;
        RegionPosition = regionPosition;
        if (!hasParachute) SpawnHeight = 0;

        StateManager = new BonusStateManager(this);
        Logger = Log.Logger.ForType(GetType());
    }

    protected ILogger Logger { get; }
    protected bool CanTake { get; private set; }

    public abstract BonusType Type { get; }
    public abstract IEntity? Entity { get; protected set; }
    public abstract IEntity? RegionEntity { get; protected set; }
    public abstract BonusConfigComponent ConfigComponent { get; }

    public Battle Battle { get; }
    public BonusStateManager StateManager { get; }
    public float SpawnHeight { get; } = 30; // todo map dependent?

    public Vector3 RegionPosition { get; }
    public Vector3 SpawnPosition => RegionPosition with { Y = RegionPosition.Y + SpawnHeight };

    public virtual async Task Take(BattleTank battleTank) {
        if (Entity == null) {
            Logger.Error("{Connection} wanted to take nonexistent bonus", battleTank.BattlePlayer.PlayerConnection);
            return;
        }

        foreach (IPlayerConnection connection in Battle.Players
                     .Where(battlePlayer => battlePlayer.InBattle)
                     .Select(battlePlayer => battlePlayer.PlayerConnection)) {
            await connection.Send(new BonusTakenEvent(), Entity);
            await connection.Unshare(Entity);
        }

        battleTank.Statistics.BonusesTaken++;
        Entity = null;
        CanTake = true;
    }

    public abstract Task Spawn();

    public virtual Task Drop() => Spawn();

    public virtual Task Tick() => StateManager.Tick();
}
