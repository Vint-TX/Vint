using System.Collections.Frozen;
using System.Reflection;
using Vint.Core.Battles.Modules.Types;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules;

public static class ModuleRegistry {
    static ModuleRegistry() {
        System.Type[] modules = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsDefined(typeof(ModuleIdAttribute)))
            .ToArray();

        Dictionary<long, IBattleModuleBuilder> idToBuilder = new(modules.Length);

        foreach (System.Type module in modules) {
            long id = module.GetCustomAttribute<ModuleIdAttribute>()!.Id;

            IBattleModuleBuilder battleModuleBuilder =
                (IBattleModuleBuilder)Activator.CreateInstance(typeof(BattleModuleBuilder<>).MakeGenericType(module))!;

            idToBuilder[id] = battleModuleBuilder;
        }

        IdToBuilder = idToBuilder.ToFrozenDictionary();
    }

    static BattleModuleBuilder<InDevModule> Fallback { get; } = new();
    static FrozenDictionary<long, IBattleModuleBuilder> IdToBuilder { get; }

    public static BattleModule Get(long id) => IdToBuilder
        .GetValueOrDefault(id, Fallback)
        .Build();

    class BattleModuleBuilder<T> : IBattleModuleBuilder where T : BattleModule, new() {
        public BattleModule Build() => new T();
    }

    interface IBattleModuleBuilder {
        BattleModule Build();
    }
}
