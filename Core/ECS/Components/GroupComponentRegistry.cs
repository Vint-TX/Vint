using System.Reflection;
using Vint.Core.ECS.Components.Group;

namespace Vint.Core.ECS.Components;

public static class GroupComponentRegistry {
    static Dictionary<Type, Dictionary<long, GroupComponent>> Groups { get; } = new();

    public static T FindOrCreateGroup<T>(long key) where T : GroupComponent =>
        (T)FindOrCreateGroup(typeof(T), key);

    public static GroupComponent FindOrCreateGroup(Type groupType, long key) {
        if (!Groups.TryGetValue(groupType, out Dictionary<long, GroupComponent>? keyToComponent))
            Groups[groupType] = keyToComponent = new Dictionary<long, GroupComponent>(1);

        if (keyToComponent.TryGetValue(key, out GroupComponent? component))
            return component;

        keyToComponent[key] = component = CreateGroupComponent(groupType, key);
        return component;
    }

    public static GroupComponent FindOrRegisterGroup(GroupComponent groupComponent) {
        Type type = groupComponent.GetType();
        long key = groupComponent.Key;

        if (!Groups.TryGetValue(type, out Dictionary<long, GroupComponent>? keyToComponent))
            Groups[type] = keyToComponent = new Dictionary<long, GroupComponent>();

        if (keyToComponent.TryGetValue(key, out GroupComponent? component))
            return component;

        keyToComponent[key] = component = groupComponent;
        return component;
    }

    static GroupComponent CreateGroupComponent(Type groupType, long key) {
        ConstructorInfo? constructor = groupType.GetConstructor([typeof(long)]);

        return constructor == null
            ? throw new ComponentInstantiatingException(groupType)
            : (GroupComponent)constructor.Invoke([key]);
    }
}

public class ComponentInstantiatingException(
    Type type
) : Exception(type.FullName);
