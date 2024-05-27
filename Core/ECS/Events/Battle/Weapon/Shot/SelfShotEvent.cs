using LinqToDB;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot;

[ProtocolId(5440037691022467911)]
public class SelfShotEvent : ShotEvent, IServerEvent {
    [ProtocolIgnore] protected virtual RemoteShotEvent RemoteEvent => new() {
        ShotDirection = ShotDirection,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public virtual async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank) return;

        IEntity tank = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            await playerConnection.Send(RemoteEvent, tank);

        if (battlePlayer.Tank?.WeaponHandler is SmokyWeaponHandler smokyHandler)
            smokyHandler.OnShot(ShotId);

        foreach (IShotModule shotModule in battlePlayer.Tank!.Modules.OfType<IShotModule>())
            await shotModule.OnShot();

        await using DbConnection db = new();
        await db.Statistics
            .Where(stats => stats.PlayerId == connection.Player.Id)
            .Set(stats => stats.Shots, stats => stats.Shots + 1)
            .UpdateAsync();
    }
}
