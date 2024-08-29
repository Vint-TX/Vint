using System.Numerics;
using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Effect.Mine;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Physics;
using Vint.Core.Server;

namespace Vint.Core.Battles.Effects;

public class MineEffect(
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
    BattleTank tank,
    int level
) : WeaponEffect(tank, level), IMineEffect {
    public override ModuleWeaponHandler WeaponHandler { get; protected set; } = null!;

    public BattleTank Owner => Tank;
    public int Index { get; } = index;
    public Vector3 Position { get; private set; }
    public float TriggeringArea { get; } = triggeringArea;
    bool WaitingForExplosion { get; set; }

    public override async Task Activate() {
        if (IsActive) return;

        RayClosestHitHandler hitHandler = new();
        Battle.Simulation.RayCast(Tank.Position, -Vector3.UnitY, 655.36f, ref hitHandler);

        if (!hitHandler.ClosestHit.HasValue)
            return;

        Tank.Effects.Add(this);

        Position = hitHandler.ClosestHit.Value + Vector3.UnitY;
        WeaponEntity = Entity = new MineEffectTemplate().Create(Tank.BattlePlayer, Duration, Position, Battle.Properties.FriendlyFire,
            beginHideDistance, hideRange, TriggeringArea, impact, minSplashDamagePercent, radiusOfMaxSplashDamage, radiusOfMinSplashDamage);

        WeaponHandler = new MineWeaponHandler(Tank, TimeSpan.Zero, marketEntity, WeaponEntity, true, radiusOfMaxSplashDamage, radiusOfMinSplashDamage,
            minSplashDamagePercent, maxDamage, minDamage, Explode);

        await ShareToAllPlayers();

        Schedule(activationTime, async () => {
            await Entity.AddComponent<EffectActiveComponent>();

            if (!Battle.MineProcessor.AddMine(this))
                await ForceDeactivate();
        });

        foreach (IPlayerConnection connection in Battle.Players.Where(player => player.InBattle).Select(player => player.PlayerConnection))
            await connection.Send(new MineDropEvent(), Entity);

        CanBeDeactivated = false;
    }

    public override async Task Deactivate() {
        if (!IsActive || !CanBeDeactivated) return;

        Battle.MineProcessor.RemoveMine(Index);
        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }

    public async Task ForceDeactivate() {
        CanBeDeactivated = true;
        await Deactivate();
    }

    public void TryExplode() {
        if (WaitingForExplosion || !IsActive)
            return;

        WaitingForExplosion = true;
        Schedule(explosionDelay, async () => await Tank.BattlePlayer.PlayerConnection.Send(new MineTryExplosionEvent(), Entity));
    }

    public async Task Explode() {
        if (!WaitingForExplosion || !IsActive)
            return;

        foreach (IPlayerConnection connection in Battle.Players.Where(player => player.InBattle).Select(player => player.PlayerConnection))
            await connection.Send(new MineExplosionEvent(), Entity);

        await ForceDeactivate();
    }
}
