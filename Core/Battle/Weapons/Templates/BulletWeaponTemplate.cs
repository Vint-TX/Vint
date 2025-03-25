using Vint.Core.Battle.Player;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Weapons.Templates;

public abstract class BulletWeaponTemplate : DiscreteWeaponTemplate {
    protected override IEntity Create(string configPath, IEntity tank, Tanker tanker) {
        IEntity entity = base.Create(configPath, tank, tanker);

        entity.AddComponent<WeaponBulletShotComponent>(configPath);
        return entity;
    }
}
