using Vint.Core.ECS.Components;
using Vint.Core.Structures;

namespace Vint.Core.ECS.Entities;

public class EntityComponentStorage(
    IEntity entity,
    IEnumerable<IComponent> components
) {
    ConcurrentList<KeyValuePair<Type, IComponent>> ComponentsInternal { get; } = new(components
        .Select(component => KeyValuePair.Create(component.GetType(), component))
        .ToList());

    public IEnumerable<IComponent> Components => ComponentsInternal.Select(kvp => kvp.Value);

    public void AddComponent(IComponent component) {
        Type type = component.GetType();
        KeyValuePair<Type, IComponent> kvp = new(type, component);

        AssertComponentNotFound(type);
        ComponentsInternal.Add(kvp);
    }

    public bool HasComponent(Type componentType) =>
        ComponentsInternal.Select(kvp => kvp.Key).Any(type => type == componentType);

    public IComponent GetComponent(Type componentType) {
        try {
            return ComponentsInternal.First(kvp => kvp.Key == componentType).Value;
        } catch (InvalidOperationException) {
            throw new ComponentNotFoundException(entity, componentType);
        }
    }

    public IComponent? GetComponentOrNull(Type componentType) {
        try {
            return GetComponent(componentType);
        } catch (ComponentNotFoundException) {
            return null;
        }
    }

    public void ChangeComponent(IComponent component) {
        Type type = component.GetType();
        RemoveComponent(type);
        AddComponent(component);
    }

    public void RemoveComponent(Type componentType) {
        if (ComponentsInternal.RemoveAll(kvp => kvp.Key == componentType) <= 0)
            throw new ComponentNotFoundException(entity, componentType);
    }

    void AssertComponentFound(Type componentType) {
        if (ComponentsInternal.Select(kvp => kvp.Key).All(type => componentType != type))
            throw new ComponentNotFoundException(entity, componentType);
    }

    void AssertComponentNotFound(Type componentType) {
        if (ComponentsInternal.Select(kvp => kvp.Key).Any(type => componentType == type))
            throw new ComponentAlreadyExistsInEntityException(entity, componentType);
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
