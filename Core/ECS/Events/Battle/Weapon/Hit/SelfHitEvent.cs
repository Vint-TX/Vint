using LinqToDB;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
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

    public virtual void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if (!battlePlayer.InBattleAsTank || !battle.Properties.DamageEnabled) return;

        BattleTank battleTank = battlePlayer.Tank!;
        IEntity weapon = entities.Single();

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            playerConnection.Send(RemoteEvent, weapon);

        if (Targets == null) return;

        WeaponHandler weaponHandler = battleTank.WeaponHandler;

        if (weaponHandler is HammerWeaponHandler hammerHandler) {
            hammerHandler.Fire(Targets);
            return;
        }

        foreach (HitTarget target in Targets) {
            try {
                weaponHandler.Fire(target);
            } catch (NotImplementedException) {
                ChatUtils.SendMessage($"{weaponHandler.GetType().Name}.Fire method is not implemented yet",
                    battle.BattleChatEntity,
                    [connection],
                    null);
            }
        }

        using DbConnection db = new();

        db.Statistics
            .Where(stats => stats.PlayerId == connection.Player.Id)
            .Set(stats => stats.Hits, stats => stats.Hits + Targets.Count)
            .Update();
    }
}