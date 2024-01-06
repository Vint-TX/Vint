using Serilog;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Events;
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

    public HashSet<IPlayerConnection> SharedPlayers { get; } = [];
    Dictionary<Type, IComponent> TypeToComponent { get; } = components.ToDictionary(c => c.GetType());

    public long Id {
        get => id;
        set => id = value;
    }

    public TemplateAccessor? TemplateAccessor => templateAccessor;
    public IEnumerable<IComponent> Components => TypeToComponent.Values.ToHashSet();

    public EntityShareCommand ToShareCommand() => new(Id, TemplateAccessor, Components.ToArray());

    public EntityUnshareCommand ToUnshareCommand() => new(this);

    public void Share(IPlayerConnection connection) {
        lock (SharedPlayers) {
            if (!SharedPlayers.Add(connection))
                throw new ArgumentException($"{this} already shared to {connection}");
        }

        Logger.Debug("Sharing {Entity} to {Connection}", this, connection);

        connection.Send(ToShareCommand());
        connection.SharedEntities.Add(this);
    }

    public void Unshare(IPlayerConnection connection) {
        lock (SharedPlayers) {
            if (!SharedPlayers.Remove(connection))
                throw new ArgumentException($"{this} is not shared to {connection}");
        }

        Logger.Debug("Unsharing {Entity} from {Connection}", this, connection);

        connection.SharedEntities.Remove(this);
        connection.Send(ToUnshareCommand());
    }

    public void AddComponent(IComponent component) => AddComponent(component, null);

    public bool HasComponent<T>() where T : IComponent {
        lock (TypeToComponent) {
            return TypeToComponent.TryGetValue(typeof(T), out _);
        }
    }

    public T GetComponent<T>() where T : class, IComponent {
        lock (TypeToComponent) {
            if (TypeToComponent.TryGetValue(typeof(T), out IComponent? component))
                return (component as T)!;

            throw new ArgumentException($"{this} does not have component {typeof(T)}");
        }
    }

    public void ChangeComponent<T>(Action<T> action) where T : class, IComponent {
        T component = GetComponent<T>();

        action(component);

        ChangeComponent(component, null);
    }

    public void RemoveComponent<T>() where T : IComponent => RemoveComponent<T>(null);

    public void RemoveComponent(IComponent component, IPlayerConnection? excluded = null) {
        Type type = component.GetType();

        lock (TypeToComponent) {
            if (!TypeToComponent.Remove(type))
                throw new ArgumentException($"{this} does not have component {type}");
        }

        lock (SharedPlayers) {
            foreach (IPlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
                playerConnection.Send(new ComponentRemoveCommand(this, type));
        }
    }
    
    public void Send(IEvent @event) {
        lock (SharedPlayers) {
            foreach (IPlayerConnection playerConnection in SharedPlayers)
                playerConnection.Send(@event, this);
        }
    }

    public IEntity Clone() => new Entity(Id,
        TemplateAccessor == null ? null
            : new TemplateAccessor(TemplateAccessor.Template, TemplateAccessor.ConfigPath),
        Components.ToHashSet());

    public void AddComponent(IComponent component, IPlayerConnection? excluded) {
        Type type = component.GetType();

        lock (TypeToComponent) {
            if (!TypeToComponent.TryAdd(type, component))
                throw new ArgumentException($"{this} already has component {type}");
        }

        Logger.Debug("Added {Type} component to the {Entity}", type, this);

        lock (SharedPlayers) {
            foreach (IPlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
                playerConnection.Send(new ComponentAddCommand(this, component));
        }
    }

    public bool HasComponent(IComponent component) {
        lock (TypeToComponent) {
            return TypeToComponent.TryGetValue(component.GetType(), out _);
        }
    }

    public void ChangeComponent(IComponent component, IPlayerConnection? excluded) {
        Type type = component.GetType();

        lock (TypeToComponent) {
            if (!TypeToComponent.ContainsKey(type))
                throw new ArgumentException($"{this} does not have component {type}");

            TypeToComponent[type] = component;
        }

        lock (SharedPlayers) {
            foreach (IPlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
                playerConnection.Send(new ComponentChangeCommand(this, component));
        }
    }

    public void RemoveComponent<T>(IPlayerConnection? excluded) where T : IComponent {
        Type type = typeof(T);

        lock (TypeToComponent) {
            if (!TypeToComponent.Remove(type))
                throw new ArgumentException($"{this} does not have component {typeof(T)}");
        }

        lock (SharedPlayers) {
            foreach (IPlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
                playerConnection.Send(new ComponentRemoveCommand(this, type));
        }
    }

    public override string ToString() => $"Entity {{ " +
                                         $"Id: {Id}; " +
                                         $"TemplateAccessor: {TemplateAccessor}; " +
                                         $"Components {{ {Components.ToString(false)} }} }}";
}