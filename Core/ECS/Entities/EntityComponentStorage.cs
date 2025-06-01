using System.Collections.Concurrent;
using Vint.Core.ECS.Components;

namespace Vint.Core.ECS.Entities;

public class EntityComponentStorage : IDisposable {
    int _lastIndex;
    bool _disposed;

    public EntityComponentStorage(IEntity entity, IEnumerable<IComponent> components) {
        Entity = entity;

        TypeToComponent =
            new ConcurrentDictionary<Type, ComponentWithIndex>(components.ToDictionary(c => c.GetType(),
                c => new ComponentWithIndex(c, GenerateIndex())));
    }

    IEntity Entity { get; }
    ConcurrentDictionary<Type, ComponentWithIndex> TypeToComponent { get; }

    public IEnumerable<IComponent> SortedComponents {
        get {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return TypeToComponent.Values.OrderBy(c => c.Index).Select(c => c.Component);
        }
    }

    public IEnumerable<IComponent> Components {
        get {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return TypeToComponent.Values.Select(c => c.Component);
        }
    }

    int GenerateIndex() => Interlocked.Increment(ref _lastIndex);

    public void AddComponent(IComponent component) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Type type = component.GetType();
        ComponentWithIndex componentWithIndex = new(component, GenerateIndex());

        if (!TypeToComponent.TryAdd(type, componentWithIndex))
            throw new ComponentAlreadyExistsInEntityException(Entity, type);
    }

    public bool HasComponent(Type componentType) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return TypeToComponent.ContainsKey(componentType);
    }

    public IComponent GetComponent(Type componentType) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return TypeToComponent.TryGetValue(componentType, out ComponentWithIndex componentWithIndex)
            ? componentWithIndex.Component
            : throw new ComponentNotFoundException(Entity, componentType);
    }

    public IComponent? GetComponentOrNull(Type componentType) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try {
            return GetComponent(componentType);
        } catch (ComponentNotFoundException) {
            return null;
        }
    }

    public void ChangeComponent(IComponent component) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Type type = component.GetType();

        if (TypeToComponent.TryGetValue(type, out ComponentWithIndex componentWithIndex))
            TypeToComponent.TryUpdate(type, new ComponentWithIndex(component, GenerateIndex()), componentWithIndex);
        else throw new ComponentNotFoundException(Entity, type);
    }

    public void RemoveComponent(Type componentType, out IComponent component) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!TypeToComponent.Remove(componentType, out ComponentWithIndex componentWithIndex))
            throw new ComponentNotFoundException(Entity, componentType);

        component = componentWithIndex.Component;
    }

    readonly record struct ComponentWithIndex(
        IComponent Component,
        int Index
    );


    public void Dispose() {
        if (_disposed) return;
        _disposed = true;

        TypeToComponent.Clear();
        GC.SuppressFinalize(this);
    }

    ~EntityComponentStorage() => Dispose();
}

public class ComponentAlreadyExistsInEntityException(
    IEntity entity,
    Type componentType
) : Exception($"{componentType.Name} entity={entity}");

public class ComponentNotFoundException(
    IEntity entity,
    Type componentType
) : ArgumentException($"{componentType.Name} entity={entity}");
