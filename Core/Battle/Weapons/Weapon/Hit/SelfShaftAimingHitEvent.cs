using Vint.Core.Battle.Weapons.Handlers.Impl;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.API;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Weapon.Hit;

[ProtocolId(8070042425022831807)]
public class SelfShaftAimingHitEvent(
    ApiServer apiServer
) : SelfHitEvent(apiServer) {
    public float HitPower { get; set; }

    [ProtocolIgnore] protected override RemoteShaftAimingHitEvent RemoteEvent => new() {
        HitPower = HitPower,
        Targets = Targets,
        StaticHit = StaticHit,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public override async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await base.Execute(connection, entities);

        if (WeaponHandler is not ShaftWeaponHandler shaft) return;

        shaft.Reset();
    }
}
