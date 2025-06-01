using System.Numerics;
using JetBrains.Annotations;
using Vint.Core.Battle.Flags.Components;
using Vint.Core.Battle.Flags.State;
using Vint.Core.Battle.Flags.Templates;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Flags;

[MustDisposeResource]
public class Flag : IDisposable {
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

    public void Dispose() {
        Entity.Dispose();
        PedestalEntity.Dispose();
        GC.SuppressFinalize(this);
    }

    ~Flag() => Dispose();
}
