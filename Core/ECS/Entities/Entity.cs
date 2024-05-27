using System.Collections.Concurrent;
using System.Diagnostics;
using ConcurrentCollections;
using Serilog;
using Vint.Core.Config;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Commands;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Entities;

public class Entity(
    long id,
    TemplateAccessor? templateAccessor,
    IEnumerable<IComponent> components
) : IEntity {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(Entity));
    ConcurrentDictionary<Type, IComponent> TypeToComponent { get; } = new(components.ToDictionary(c => c.GetType()));

    public ConcurrentHashSet<IPlayerConnection> SharedPlayers { get; } = [];

    public long Id {
        get => id;
        set => id = value;
    }

    public TemplateAccessor? TemplateAccessor => templateAccessor;
    public IEnumerable<IComponent> Components => TypeToComponent.Values.ToHashSet();

    public EntityShareCommand ToShareCommand() => new(Id, TemplateAccessor, Components.ToArray());

    public EntityUnshareCommand ToUnshareCommand() => new(this);

    public async Task Share(IPlayerConnection connection) {
        Logger.Debug("Sharing {Entity} to {Connection}", this, connection);

        if (!SharedPlayers.Add(connection)) {
            Logger.Warning("{Entity} is already shared to {Connection}", this, connection);
            Debugger.Break();
        }

        await connection.Send(ToShareCommand());
        connection.SharedEntities.Add(this);
    }

    public async Task Unshare(IPlayerConnection connection) {
        Logger.Debug("Unsharing {Entity} from {Connection}", this, connection);

        if (!SharedPlayers.TryRemove(connection)) {
            Logger.Warning("{Entity} is not shared to {Connection}", this, connection);
            Debugger.Break();
        }

        connection.SharedEntities.TryRemove(this);
        await connection.Send(ToUnshareCommand());
    }

    public T GetComponent<T>() where T : class, IComponent => (T)GetComponent(typeof(T));

    public IComponent GetComponent(Type type) {
        if (TypeToComponent.TryGetValue(type, out IComponent? component))
            return component;

        throw new ArgumentException($"{this} does not have component {type}");
    }

    public Task RemoveComponentIfPresent<T>(IPlayerConnection? excluded = null) where T : class, IComponent =>
        RemoveComponentIfPresent(typeof(T), excluded);

    public Task RemoveComponentIfPresent(IComponent component, IPlayerConnection? excluded = null) =>
        RemoveComponentIfPresent(component.GetType(), excluded);

    public async Task RemoveComponentIfPresent(Type type, IPlayerConnection? excluded = null) {
        if (HasComponent(type))
            await RemoveComponent(type, excluded);
    }

    public IEntity Clone() => new Entity(Id,
        TemplateAccessor == null ? null
            : new TemplateAccessor(TemplateAccessor.Template, TemplateAccessor.ConfigPath),
        Components.ToHashSet());

    public async Task AddComponent(IComponent component, IPlayerConnection? excluded = null) {
        Type type = component.GetType();

        if (!TypeToComponent.TryAdd(type, component))
            throw new ArgumentException($"{this} already has component {type}");

        Logger.Debug("Added {Name} component to the {Entity}", type.Name, this);

        foreach (IPlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
            await playerConnection.Send(new ComponentAddCommand(this, component));
    }

    public Task AddComponent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new() => AddComponent(new T(), excluded);

    public Task AddComponent<T>(string configPath, IPlayerConnection? excluded = null) where T : class, IComponent =>
        AddComponent(ConfigManager.GetComponent<T>(configPath), excluded);

    public Task AddGroupComponent<T>(IEntity? entity = null, IPlayerConnection? excluded = null) where T : GroupComponent =>
        AddComponent((T)Activator.CreateInstance(typeof(T), entity ?? this)!, excluded);

    public Task AddComponentFrom<T>(IEntity entity, IPlayerConnection? excluded = null) where T : class, IComponent =>
        AddComponent(entity.GetComponent<T>(), excluded);

    public async Task AddComponentIfAbsent(IComponent component, IPlayerConnection? excluded = null) {
        if (!HasComponent(component))
            await AddComponent(component, excluded);
    }

    public async Task AddComponentIfAbsent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new() {
        if (!HasComponent<T>())
            await AddComponent<T>(excluded);
    }

    public bool HasComponent<T>() where T : class, IComponent =>
        HasComponent(typeof(T));

    public bool HasComponent(IComponent component) =>
        HasComponent(component.GetType());

    public bool HasComponent(Type type) => TypeToComponent.TryGetValue(type, out _);

    public async Task ChangeComponent<T>(Func<T, Task> func) where T : class, IComponent {
        T component = GetComponent<T>();

        await func(component);
        await ChangeComponent(component, null);
    }

    public async Task ChangeComponent<T>(Action<T> action) where T : class, IComponent {
        T component = GetComponent<T>();

        action(component);
        await ChangeComponent(component, null);
    }

    public async Task ChangeComponent(IComponent component, IPlayerConnection? excluded) {
        Type type = component.GetType();

        if (!TypeToComponent.ContainsKey(type))
            throw new ArgumentException($"{this} does not have component {type}");

        TypeToComponent[type] = component;

        Logger.Debug("Changed {Name} component in the {Entity}", type.Name, this);

        foreach (IPlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
            await playerConnection.Send(new ComponentChangeCommand(this, component));
    }

    public Task RemoveComponent<T>(IPlayerConnection? excluded) where T : class, IComponent =>
        RemoveComponent(typeof(T), excluded);

    public Task RemoveComponent(IComponent component, IPlayerConnection? excluded = null) =>
        RemoveComponent(component.GetType(), excluded);

    public async Task RemoveComponent(Type type, IPlayerConnection? excluded = null) {
        if (!TypeToComponent.TryRemove(type, out _))
            throw new ArgumentException($"{this} does not have component {type}");

        Logger.Debug("Removed {Name} component from the {Entity}", type.Name, this);

        foreach (IPlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
            await playerConnection.Send(new ComponentRemoveCommand(this, type));
    }

    public override string ToString() => $"Entity {{ " +
                                         $"Id: {Id}; " +
                                         $"TemplateAccessor: {TemplateAccessor}; " +
                                         $"Components {{ {Components.ToString(false)} }} }}";

    public override int GetHashCode() => Id.GetHashCode();
}
