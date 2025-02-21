using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Quests;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Player;

public abstract class BattlePlayer(
    Round round,
    IPlayerConnection connection
) : IWithConnection, IDisposable {
    public Guid Id { get; } = Guid.NewGuid();

    public abstract IEntity BattleUser { get; }
    public IPlayerConnection Connection { get; } = connection;
    public Round Round { get; } = round;

    public virtual async Task Init() {
        await Connection.Share(Round.ModeHandler.Entity, Round.Entity, Round.ChatEntity);

        if (Round.BonusProcessor != null)
            await Round.BonusProcessor.ShareEntities(Connection);

        foreach (Effect effect in Round.Tankers.SelectMany(tanker => tanker.Tank.Effects))
            await effect.ShareTo(this);

        await Connection.UserContainer.Entity.AddGroupComponent<BattleGroupComponent>(Round.Entity);
        await Connection.Share(Round.Tankers.SelectMany(player => player.Tank.Entities));
    }

    public virtual async Task DeInit() {
        await Connection.Unshare(Round.Tankers.Where(tanker => tanker != this).SelectMany(player => player.Tank.Entities));
        await Connection.UserContainer.Entity.RemoveComponent<BattleGroupComponent>();

        foreach (Effect effect in Round.Tankers.SelectMany(tanker => tanker.Tank.Effects))
            await effect.UnshareFrom(this);

        if (Round.BonusProcessor != null)
            await Round.BonusProcessor.UnshareEntities(Connection);

        await Connection.Unshare(Round.ModeHandler.Entity, Round.Entity, Round.ChatEntity);
    }

    public abstract Task OnRoundEnded(bool hasEnemies, QuestManager questManager);

    public abstract Task Tick(TimeSpan deltaTime);

    public override int GetHashCode() => Connection.GetHashCode();

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            // todo dispose entities
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BattlePlayer() => Dispose(false);
}
