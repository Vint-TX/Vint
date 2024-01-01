using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Preset;

[ProtocolId(1502886877871)]
public class PresetEquipmentComponent(
    Database.Models.Preset preset
) : IComponent {
    public long WeaponId { get; private set; } = preset.Weapon.Id;
    public long HullId { get; private set; } = preset.Hull.Id;

    public void SetWeaponId(long weaponId) {
        WeaponId = weaponId;
        RefreshComponent();
    }

    public void SetHullId(long hullId) {
        HullId = hullId;
        RefreshComponent();
    }

    void RefreshComponent() {
        try {
            preset.Entity?.RemoveComponent<PresetEquipmentComponent>();
        } finally {
            preset.Entity?.AddComponent(this);
        }
    }
}