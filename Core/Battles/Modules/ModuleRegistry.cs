using Vint.Core.Battles.Modules.Types;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules;

public static class ModuleRegistry {
    static ModuleRegistry() {
        Register<RepairKitModule>("RepairKit");
        Register<AbsorbingArmorModule>("AbsorbingArmor");
        Register<IncreasedDamageModule>("IncreasedDamage");
        Register<TurboSpeedModule>("TurboSpeed");
        Register<SonarModule>("Sonar");
        Register<ExternalImpactModule>("ExternalImpact");
        Register<KamikadzeModule>("Kamikadze");
        Register<RageModule>("Rage");
        Register<TemperatureBlockModule>("TempBlock");
        Register<EmergencyProtectionModule>("EmergencyProtection");
        Register<AcceleratedGearsModule>("AcceleratedGears");
        Register<EngineerModule>("Engineer");
        Register<BackhitDefenceModule>("BackhitDefence");
        Register<BackhitIncreaseModule>("BackhitIncrease");
        Register<AdrenalineModule>("Adrenaline");
        Register<LifeStealModule>("LifeSteal");
        Register<EnergyInjectionModule>("EnergyInjection");
        Register<JumpImpactModule>("JumpImpact");
        Register<InvulnerabilityModule>("Invulnerability");
        Register<ForceFieldModule>("ForceField");
        Register<InvisibilityModule>("Invisibility");
        Register<ExplosiveMassModule>("ExplosiveMass");
        Register<FireRingModule>("FireRing");
        Register<DroneModule>("Drone");
        Register<EMPModule>("Emp");
    }

    static BattleModuleBuilder<InDevModule> Fallback { get; } = new();
    static Dictionary<long, IBattleModuleBuilder> IdToBuilder { get; } = new();

    public static BattleModule Get(long id) => IdToBuilder.GetValueOrDefault(id, Fallback).Build();

    static void Register<T>(string name) where T : BattleModule, new() =>
        IdToBuilder[GlobalEntities.GetEntity("modules", name).Id] = new BattleModuleBuilder<T>();

    class BattleModuleBuilder<T> : IBattleModuleBuilder where T : BattleModule, new() {
        public BattleModule Build() => new T();
    }

    interface IBattleModuleBuilder {
        public BattleModule Build();
    }
}
