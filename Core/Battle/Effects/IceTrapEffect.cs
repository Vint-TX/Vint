using System.Numerics;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Effect.Mine;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Physics;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Effects;

public class IceTrapEffect(
    int index,
    IEntity marketEntity,
    TimeSpan activationTime,
    TimeSpan explosionDelay,
    float beginHideDistance,
    float hideRange,
    float triggeringArea,
    float impact,
    float minSplashDamagePercent,
    float radiusOfMaxSplashDamage,
    float radiusOfMinSplashDamage,
    float maxDamage,
    float minDamage,
    float temperatureDelta,
    float temperatureLimit,
    TimeSpan temperatureDuration,
    BattleTank tank,
    int level
) : WeaponEffect(tank, level), IMineEffect {
    public override ModuleWeaponHandler WeaponHandler { get; protected set; } = null!;
    bool WaitingForExplosion { get; set; }

    public BattleTank Owner => Tank;
    public int Index { get; } = index;
    public Vector3 Position { get; private set; }
    public float TriggeringArea { get; } = triggeringArea;

    public void TryExplode() {
        if (WaitingForExplosion || !IsActive)
            return;

        WaitingForExplosion = true;
        Schedule(explosionDelay, async () => await Tank.Tanker.Send(new MineTryExplosionEvent(), Entity));
    }

    public override async Task Activate() {
        if (IsActive) return;

        RayClosestHitHandler hitHandler = new();
        Round.Simulation.RayCast(Tank.Position, -Vector3.UnitY, 655.36f, ref hitHandler);

        if (!hitHandler.ClosestHit.HasValue ||
            Round.ModeHandler is CTFHandler ctf && !ctf.CanPlaceMine(hitHandler.ClosestHit.Value))
            return;

        Tank.Effects.Add(this);

        Position = hitHandler.ClosestHit.Value + Vector3.UnitY;

        WeaponEntity = Entity = new IceTrapEffectTemplate().Create(Tank.Tanker,
            Duration,
            Position,
            Round.Properties.FriendlyFire,
            beginHideDistance,
            hideRange,
            TriggeringArea,
            impact,
            minSplashDamagePercent,
            radiusOfMaxSplashDamage,
            radiusOfMinSplashDamage);

        WeaponHandler = new IceTrapWeaponHandler(Tank,
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
            temperatureDelta,
            temperatureLimit,
            temperatureDuration,
            Explode);

        await ShareToAllPlayers();

        Schedule(activationTime,
            async () => {
                await Entity.AddComponent<EffectActiveComponent>();

                if (!Round.MineProcessor.AddMine(this))
                    await ForceDeactivate();
            });

        await Round.Players.Send(new MineDropEvent(), Entity);
        CanBeDeactivated = false;
    }

    public override async Task Deactivate() {
        if (!IsActive ||
            !CanBeDeactivated) return;

        Round.MineProcessor.RemoveMine(Index);
        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }

    public async Task ForceDeactivate() {
        CanBeDeactivated = true;
        await Deactivate();
    }

    public async Task Explode() {
        if (!WaitingForExplosion || !IsActive)
            return;

        await Round.Players.Send(new MineExplosionEvent(), Entity);
        await ForceDeactivate();
    }
}
