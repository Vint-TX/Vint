using System.Numerics;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Components.Battle.Flag;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Battle.Flag;

namespace Vint.Core.Battle.Flags;

public class Flag {
    public Flag(Round round, IEntity team, TeamColor teamColor, Vector3 pedestalPosition, TimeSpan enemyFlagActionInterval) {
        Round = round;
        TeamColor = teamColor;
        PedestalPosition = pedestalPosition;
        EnemyFlagActionInterval = enemyFlagActionInterval;
        StateManager = new FlagStateManager(this);

        PedestalEntity = new PedestalTemplate().Create(pedestalPosition, team, Round.Entity);
        Entity = new FlagTemplate().Create(pedestalPosition, team, Round.Entity);
    }

    public FlagStateManager StateManager { get; }

    public IEntity PedestalEntity { get; }
    public IEntity Entity { get; }

    public Round Round { get; private set; }
    public TeamColor TeamColor { get; private set; }
    public Vector3 PedestalPosition { get; }
    public Vector3 Position => Entity.GetComponent<FlagPositionComponent>().Position;

    public TimeSpan EnemyFlagActionInterval { get; }

    public async Task Init() =>
        await StateManager.Init();

    public async Task Tick(TimeSpan deltaTime) =>
        await StateManager.Tick(deltaTime);
}
