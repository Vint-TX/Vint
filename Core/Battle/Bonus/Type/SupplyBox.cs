using System.Numerics;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Battle;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Bonus;

namespace Vint.Core.Battle.Bonus.Type;

public abstract class SupplyBox<T>(
    Round round,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox(round, regionPosition, hasParachute) where T : Effect, ISupplyEffect {
    protected abstract T GetEffect(BattleTank battleTank);

    public override async Task Take(BattleTank battleTank) {
        await base.Take(battleTank);

        if (!CanTake) return;

        await StateManager.SetState(new Cooldown(StateManager));

        T? effect = battleTank
            .Effects
            .OfType<T>()
            .SingleOrDefault();

        switch (effect) {
            case null:
                await GetEffect(battleTank).Activate();
                break;

            case IExtendableEffect extendableEffect:
                await extendableEffect.Extend(-1);
                break;
        }
    }
}

public abstract class SupplyBox : BonusBox {
    protected SupplyBox(Round round, Vector3 regionPosition, bool hasParachute) : base(round, regionPosition, hasParachute) {
        // ReSharper disable VirtualMemberCallInConstructor
        ConfigComponent = ConfigManager.GetComponent<BonusConfigComponent>($"battle/bonus/{Type.ToString().ToLower()}");
        RegionEntity = new Lazy<IEntity>(new BonusRegionTemplate().CreateRegular(Type, RegionPosition));
        // ReSharper restore VirtualMemberCallInConstructor
    }

    public override BonusConfigComponent ConfigComponent { get; }
    public sealed override Lazy<IEntity> RegionEntity { get; protected set; }
    public override IEntity? Entity { get; protected set; }

    public override async Task Spawn() {
        Entity = new SupplyBonusTemplate().Create(Type, SpawnPosition, RegionEntity.Value, Round.Entity);
        await StateManager.SetState(new Spawned(StateManager));
    }

    public override bool CanBeDropped(bool force) => // ??
        StateManager.CurrentState is not Spawned ||
        force && StateManager.CurrentState is Cooldown;
}
