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
    }
    
    static BattleModule Fallback => new InDevModule();
    static Dictionary<long, IBattleModuleBuilder> IdToBuilder { get; } = new();
    
    public static BattleModule Get(long id) =>
        IdToBuilder.TryGetValue(id, out IBattleModuleBuilder? builder) ? builder.Build() : Fallback;
    
    static void Register<T>(string name) where T : BattleModule, new() =>
        IdToBuilder[GlobalEntities.GetEntity("modules", name).Id] = new BattleModuleBuilder<T>();
    
    class BattleModuleBuilder<T> : IBattleModuleBuilder where T : BattleModule, new() {
        public BattleModule Build() => new T();
    }
    
    interface IBattleModuleBuilder {
        public BattleModule Build();
    }
}