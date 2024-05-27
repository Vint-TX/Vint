using System.Numerics;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.Bonus;

namespace Vint.Core.Battles.Bonus.Type;

public abstract class SupplyBox<T>(
    Battle battle,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox(battle, regionPosition, hasParachute) where T : Effect, ISupplyEffect {
    protected abstract T GetEffect(BattleTank battleTank);

    public override async Task Take(BattleTank battleTank) {
        await base.Take(battleTank);

        if (!CanTake) return;

        await StateManager.SetState(new Cooldown(StateManager));

        T? effect = battleTank.Effects.OfType<T>().SingleOrDefault();

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
    protected SupplyBox(
        Battle battle,
        Vector3 regionPosition,
        bool hasParachute) : base(battle, regionPosition, hasParachute) =>
        // ReSharper disable VirtualMemberCallInConstructor
        ConfigComponent = ConfigManager.GetComponent<BonusConfigComponent>($"battle/bonus/{Type.ToString().ToLower()}");

    public override BonusConfigComponent ConfigComponent { get; }
    public override IEntity? RegionEntity { get; protected set; }
    public override IEntity? Entity { get; protected set; }

    public override async Task Spawn() {
        Entity = new SupplyBonusTemplate().Create(Type, SpawnPosition, RegionEntity!, Battle.Entity);
        await StateManager.SetState(new Spawned(StateManager));
    }

    public async Task ShareRegion() {
        RegionEntity ??= new BonusRegionTemplate().CreateRegular(Type, RegionPosition);

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
            await battlePlayer.PlayerConnection.ShareIfUnshared(RegionEntity);
    }
}
