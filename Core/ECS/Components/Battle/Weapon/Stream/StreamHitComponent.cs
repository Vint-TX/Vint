using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(-6274985110858845212), ClientAddable, ClientRemovable]
public class StreamHitComponent : IComponent {
    public HitTarget? TankHit { get; private set; }
    public StaticHit? StaticHit { get; private set; }

    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank) return Task.CompletedTask;

        foreach (IShotModule shotModule in connection.BattlePlayer.Tank!.Modules.OfType<IShotModule>())
            shotModule.OnShot();

        return Task.CompletedTask;
    }
}
