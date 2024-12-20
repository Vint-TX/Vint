using Vint.Core.Battles.Weapons;
using Vint.Core.Discord;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(8070042425022831807)]
public class SelfShaftAimingHitEvent(
    DiscordBot? discordBot
) : SelfHitEvent(discordBot) {
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
