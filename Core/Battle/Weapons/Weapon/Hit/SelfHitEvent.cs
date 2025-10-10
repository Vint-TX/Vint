using LinqToDB;
using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Weapons.Handlers;
using Vint.Core.Battle.Weapons.Handlers.Impl;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Logging;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Weapons.Weapon.Hit;

[ProtocolId(8814758840778124785)]
public class SelfHitEvent(
    ApiServer apiServer
) : HitEvent, IServerEvent {
    [ProtocolIgnore] protected virtual RemoteHitEvent RemoteEvent => new() {
        Targets = Targets,
        StaticHit = StaticHit,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    [ProtocolIgnore] protected bool IsProceeded { get; private set; }
    [ProtocolIgnore] protected IWeaponHandler WeaponHandler { get; private set; } = null!;

    public virtual async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IsProceeded = true;

        if (connection.LobbyPlayer?.Tanker is not HumanTanker human) {
            IsProceeded = false;
            return;
        }

        BotTanker? bot = null;
        IEntity weaponEntity = entities.Single();
        IEntity tankEntity;

        if (weaponEntity.TryGetComponent(out TankGroupComponent? tankGroupComponent))
            tankEntity = EntityRegistry.Get(tankGroupComponent.Key);
        else { // ECS...... :(
            long userId = weaponEntity.GetComponent<UserGroupComponent>().Key;
            Tanker? tanker = human.Round.Tankers.FirstOrDefault(tanker => tanker.Connection.UserContainer.Id == userId);

            if (tanker == null) {
                IsProceeded = false;
                return;
            }

            tankEntity = tanker.Tank.Entities.Tank;
        }

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank) {
                IsProceeded = false;
                return;
            }
        } else if (!human.ControlledBots.TryGetValue(autopilotComponent.Id, out bot)) {
            IsProceeded = false;
            return;
        }

        Round round = human.Round;
        BattleTank tank = bot?.Tank ?? human.Tank;
        WeaponHandler = GetWeaponHandler(tank, weaponEntity);

        if (!Validate(connection, WeaponHandler)) {
            IsProceeded = false;

            if (!human.Reported) {
                await apiServer.Report($"{connection.Player.Username} is suspected to be cheating", "Server");
                human.Reported = true;
            }

            return;
        }

        await round.Humans
            .Where(player => player != human)
            .Send(RemoteEvent, weaponEntity);

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
        if (Targets?.Count > weaponHandler.MaxHitTargets) {
            connection.Logger
                .ForType(GetType())
                .Warning("Suspicious behaviour: hit targets count is greater than max hit targets count: {Current} > {Max} ({WeaponHandlerName})",
                    Targets?.Count,
                    weaponHandler.MaxHitTargets,
                    weaponHandler.GetType().Name);

            return false;
        }

        return true;
    }

    static IWeaponHandler GetWeaponHandler(BattleTank tank, IEntity weaponEntity) {
        if (weaponEntity.HasComponent<TankPartComponent>())
            return tank.WeaponHandler;

        return tank.Effects
                   .OfType<WeaponEffect>()
                   .SingleOrDefault(effect => effect.WeaponEntity == weaponEntity)?.WeaponHandler ??
               throw new InvalidOperationException($"Not found weapon handler for {weaponEntity}");
    }
}
