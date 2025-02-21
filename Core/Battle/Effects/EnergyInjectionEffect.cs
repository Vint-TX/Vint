using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Effects;

public class EnergyInjectionEffect(
    BattleTank tank,
    int level,
    float reloadPercent
) : Effect(tank, level) {
    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new EnergyInjectionEffectTemplate().Create(Tank.Tanker, Duration, reloadPercent);
        await ShareToAllPlayers();

        IEntity weaponEntity = Tank.Entities.Weapon;

        await ReloadWeapon();
        await Tank.Tanker.Send(new ExecuteEnergyInjectionEvent(), Entity, weaponEntity);

        await Deactivate();
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }

    async Task ReloadWeapon() {
        if (Tank.WeaponHandler is HammerWeaponHandler hammer)
            await hammer.FillMagazine();
    }
}
