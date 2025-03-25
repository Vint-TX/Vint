using LinqToDB;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot;

[ProtocolId(5440037691022467911)]
public class SelfShotEvent : ShotEvent, IServerEvent {
    [ProtocolIgnore] protected virtual RemoteShotEvent RemoteEvent => new() {
        ShotDirection = ShotDirection,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public virtual async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        IEntity weaponEntity = entities.Single();

        if (tanker == null || tanker.Tank.WeaponHandler.BattleEntity != weaponEntity)
            return;

        Round round = tanker.Round;
        BattleTank tank = tanker.Tank;

        await round.Players
            .Where(player => player != tanker)
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
