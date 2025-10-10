using LinqToDB;
using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Weapons.Handlers.Impl;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Weapon.Shot;

[ProtocolId(5440037691022467911)]
public class SelfShotEvent : ShotEvent, IServerEvent {
    [ProtocolIgnore] protected virtual RemoteShotEvent RemoteEvent => new() {
        ShotDirection = ShotDirection,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    // I wish ECS were here...
    public virtual async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return;

        BotTanker? bot = null;
        IEntity weaponEntity = entities.Single();
        IEntity tankEntity = EntityRegistry.Get(weaponEntity.GetComponent<TankGroupComponent>().Key);

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank)
                return;
        } else if (!human.ControlledBots.TryGetValue(autopilotComponent.Id, out bot))
            return;

        Round round = human.Round;
        BattleTank tank = bot?.Tank ?? human.Tank;

        await round.Humans
            .Where(player => player != human)
            .Send(RemoteEvent, weaponEntity);

        if (tank.WeaponHandler is SmokyWeaponHandler smokyHandler)
            smokyHandler.OnShot(ShotId);

        foreach (IShotModule shotModule in tank.Modules.OfType<IShotModule>())
            await shotModule.OnShot();

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == connection.Player.Id)
            .Set(stats => stats.Shots, stats => stats.Shots + 1)
            .UpdateAsync();
    }
}
