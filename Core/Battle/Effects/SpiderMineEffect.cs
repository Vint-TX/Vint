using System.Numerics;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Effect.Mine;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Physics;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Effects;

public class SpiderMineEffect(
    int index,
    IEntity marketEntity,
    TimeSpan activationTime,
    float targetingDistance,
    float beginHideDistance,
    float hideRange,
    float impact,
    float minSplashDamagePercent,
    float radiusOfMaxSplashDamage,
    float radiusOfMinSplashDamage,
    float maxDamage,
    float minDamage,
    float speed,
    float acceleration,
    float energy,
    float idleEnergyDrain,
    float chasingEnergyDrain,
    BattleTank tank,
    int level
) : WeaponEffect(tank, level) {
    public override ModuleWeaponHandler WeaponHandler { get; protected set; } = null!;

    public int Index { get; } = index;
    public SpiderState State { get; set; } = SpiderState.Idling;
    double Energy { get; set; } = energy;

    public override async Task Activate() {
        if (IsActive) return;

        RayClosestHitHandler hitHandler = new();
        Round.Simulation.RayCast(Tank.Position, -Vector3.UnitY, 655.36f, ref hitHandler);

        if (!hitHandler.ClosestHit.HasValue)
            return;

        Tank.Effects.Add(this);
        Vector3 position = hitHandler.ClosestHit.Value;

        WeaponEntity = Entity = new SpiderEffectTemplate().Create(Tank.Tanker,
            Duration,
            position,
            Round.Properties.FriendlyFire,
            beginHideDistance,
            hideRange,
            impact,
            minSplashDamagePercent,
            radiusOfMaxSplashDamage,
            radiusOfMinSplashDamage,
            targetingDistance,
            speed,
            acceleration);

        WeaponHandler = new SpiderMineWeaponHandler(Tank,
            Round.DamageCalculator,
            TimeSpan.Zero,
            marketEntity,
            WeaponEntity,
            true,
            radiusOfMaxSplashDamage,
            radiusOfMinSplashDamage,
            minSplashDamagePercent,
            maxDamage,
            minDamage,
            Explode);

        await ShareToAllPlayers();
        Schedule(activationTime, async () => await Entity.AddComponent<EffectActiveComponent>());

        await Round.Players.Send(new MineDropEvent(), Entity);
        CanBeDeactivated = false;
    }

    public override async Task Deactivate() {
        if (!CanBeDeactivated || !IsActive)
            return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }

    public async Task ForceDeactivate() {
        CanBeDeactivated = true;
        await Deactivate();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (!IsActive) return;

        await DrainEnergy(deltaTime);
    }

    async Task TryExplode() {
        if (!IsActive) return;

        await Tank.Tanker.Connection.Send(new MineTryExplosionEvent(), Entity);
    }

    async Task Explode() {
        if (!IsActive) return;

        await Round.Players.Send(new MineExplosionEvent(), Entity);
        await ForceDeactivate();
    }

    async Task DrainEnergy(TimeSpan deltaTime) {
        if (Energy <= 0)
            return;

        double energyDelta = State switch {
                                 SpiderState.Idling => idleEnergyDrain,
                                 SpiderState.Chasing => chasingEnergyDrain,
                                 _ => 0
                             } * deltaTime.TotalSeconds;

        Energy -= energyDelta;

        if (Energy <= 0)
            await TryExplode();
    }
}

public enum SpiderState {
    Idling,
    Chasing
}
