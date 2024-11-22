using System.Collections.Concurrent;
using Vint.Core.ECS.Components;

namespace Vint.Core.ECS.Entities;

public class EntityComponentStorage {
    int _lastIndex;

    public EntityComponentStorage(IEntity entity, IEnumerable<IComponent> components) {
        Entity = entity;

        TypeToComponent =
            new ConcurrentDictionary<Type, ComponentWithIndex>(components.ToDictionary(c => c.GetType(),
                c => new ComponentWithIndex(c, GenerateIndex())));
    }

    IEntity Entity { get; }
    ConcurrentDictionary<Type, ComponentWithIndex> TypeToComponent { get; }

    public IEnumerable<IComponent> SortedComponents => TypeToComponent
        .Values
        .OrderBy(c => c.Index)
        .Select(c => c.Component);
    public IEnumerable<IComponent> Components => TypeToComponent.Values.Select(c => c.Component);

    int GenerateIndex() => Interlocked.Increment(ref _lastIndex);

    public void AddComponent(IComponent component) {
        Type type = component.GetType();
        ComponentWithIndex componentWithIndex = new(component, GenerateIndex());

        if (!TypeToComponent.TryAdd(type, componentWithIndex))
            throw new ComponentAlreadyExistsInEntityException(Entity, type);
    }

    public bool HasComponent(Type componentType) => TypeToComponent.ContainsKey(componentType);

    public IComponent GetComponent(Type componentType) =>
        TypeToComponent.TryGetValue(componentType, out ComponentWithIndex componentWithIndex)
            ? componentWithIndex.Component
            : throw new ComponentNotFoundException(Entity, componentType);

    public IComponent? GetComponentOrNull(Type componentType) {
        try {
            return GetComponent(componentType);
        } catch (ComponentNotFoundException) {
            return null;
        }
    }

    public void ChangeComponent(IComponent component) {
        Type type = component.GetType();

        if (TypeToComponent.TryGetValue(type, out ComponentWithIndex componentWithIndex))
            TypeToComponent.TryUpdate(type, new ComponentWithIndex(component, GenerateIndex()), componentWithIndex);
        else throw new ComponentNotFoundException(Entity, type);
    }

    public void RemoveComponent(Type componentType) {
        if (!TypeToComponent.Remove(componentType, out _))
            throw new ComponentNotFoundException(Entity, componentType);
    }

    readonly struct ComponentWithIndex(
        IComponent component,
        int index
    ) {
        public IComponent Component { get; } = component;
        public int Index { get; } = index;
    }
}

public class ComponentAlreadyExistsInEntityException(
    IEntity entity,
    Type componentType
) : Exception($"{componentType.Name} entity={entity}");

public class ComponentNotFoundException(
    IEntity entity,
    Type componentType
) : ArgumentException($"{componentType.Name} entity={entity}");
