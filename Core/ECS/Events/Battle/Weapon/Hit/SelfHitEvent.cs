using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(8814758840778124785)]
public class SelfHitEvent : HitEvent, IServerEvent {
    protected virtual RemoteHitEvent RemoteEvent => new() {
        Targets = Targets,
        StaticHit = StaticHit,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public virtual void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;

        if (!battlePlayer.InBattleAsTank) return;

        BattleTank battleTank = battlePlayer.Tank!;

        if (battleTank.StateManager.CurrentState is Dead && battleTank.WeaponHandler is not BulletWeaponHandler) return;

        IEntity weapon = entities.Single();
        Battles.Battle battle = battlePlayer.Battle;

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            playerConnection.Send(RemoteEvent, weapon);

        // todo
    }
}