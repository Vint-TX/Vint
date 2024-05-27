using LinqToDB;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(8814758840778124785)]
public class SelfHitEvent : HitEvent, IServerEvent {
    [ProtocolIgnore] protected virtual RemoteHitEvent RemoteEvent => new() {
        Targets = Targets,
        StaticHit = StaticHit,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    [ProtocolIgnore] protected bool IsProceeded { get; private set; }
    [ProtocolIgnore] protected IWeaponHandler WeaponHandler { get; private set; } = null!;

    public virtual async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IsProceeded = true;

        if (!connection.InLobby) {
            IsProceeded = false;
            return;
        }

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if (!battlePlayer.InBattleAsTank || !battle.Properties.DamageEnabled) {
            IsProceeded = false;
            return;
        }

        BattleTank battleTank = battlePlayer.Tank!;
        IEntity weapon = entities.Single();
        WeaponHandler = GetWeaponHandler(battleTank, weapon);

        if (!Validate(connection, WeaponHandler)) {
            IsProceeded = false;
            await battlePlayer.OnAntiCheatSuspected();
            return;
        }

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            await playerConnection.Send(RemoteEvent, weapon);

        if (Targets == null) return;

        switch (WeaponHandler) {
            case HammerWeaponHandler hammerHandler:
                await hammerHandler.Fire(Targets);
                return;

            case SmokyWeaponHandler smokyHandler:
                smokyHandler.OnHit(ShotId, StaticHit != null);
                break;
        }

        for (int i = 0; i < Targets.Count; i++) {
            HitTarget target = Targets[i];
            await WeaponHandler.Fire(target, i);
        }

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == connection.Player.Id)
            .Set(stats => stats.Hits, stats => stats.Hits + Targets.Count)
            .UpdateAsync();
    }

    bool Validate(IPlayerConnection connection, IWeaponHandler weaponHandler) {
        DateTimeOffset currentHitTime = DateTimeOffset.UtcNow;
        /*DateTimeOffset previousHitTime = weaponHandler.LastHitTime;
        double timePassedMs = (currentHitTime - previousHitTime).TotalMilliseconds + connection.Ping;

        if (weaponHandler is not StreamWeaponHandler && timePassedMs < weaponHandler.Cooldown.TotalMilliseconds) {
            connection.Logger.ForType(GetType())
                .Warning("Suspicious behaviour: cooldown has not passed: {TimePassed} < {Cooldown} ({WeaponHandlerName})",
                    timePassedMs,
                    weaponHandler.Cooldown.TotalMilliseconds,
                    weaponHandler.GetType().Name);

            return false;
        }*/

        weaponHandler.LastHitTime = currentHitTime;

        if (Targets?.Count > weaponHandler.MaxHitTargets) {
            connection.Logger.ForType(GetType())
                .Warning("Suspicious behaviour: hit targets count is greater than max hit targets count: {Current} > {Max} ({WeaponHandlerName})",
                    Targets?.Count,
                    weaponHandler.MaxHitTargets,
                    weaponHandler.GetType().Name);

            return false;
        }

        return true;
    }

    static IWeaponHandler GetWeaponHandler(BattleTank tank, IEntity weapon) {
        IModuleWeaponEffect? module = tank.Effects.SingleOrDefault(module => module.Entities.Contains(weapon)) as IModuleWeaponEffect;

        return module?.WeaponHandler ?? tank.WeaponHandler as IWeaponHandler;
    }
}
