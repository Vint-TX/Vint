using Serilog;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Events;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Commands;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint.Core.ECS.Entities;

public class Entity(
    long id,
    TemplateAccessor? templateAccessor,
    IEnumerable<IComponent> components
) : IEntity, IInternalEntity {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(Entity));

    Dictionary<Type, IComponent> TypeToComponent { get; } = components.ToDictionary(c => c.GetType());
    HashSet<PlayerConnection> SharedPlayers { get; } = new();

    public long Id => id;
    public TemplateAccessor? TemplateAccessor => templateAccessor;
    public HashSet<IComponent> Components => TypeToComponent.Values.ToHashSet();

    public EntityShareCommand ToShareCommand() => new(Id, TemplateAccessor, Components.ToArray());

    public EntityUnshareCommand ToUnshareCommand() => new(this);

    public void Share(PlayerConnection connection) {
        lock (SharedPlayers) {
            if (!SharedPlayers.Add(connection))
                throw new ArgumentException($"{this} already shared to {connection}");
        }

        Logger.Debug("Sharing {Entity} to {Connection}", this, connection);

        connection.Send(ToShareCommand());
    }

    public void Unshare(PlayerConnection connection) {
        lock (SharedPlayers) {
            if (!SharedPlayers.Remove(connection))
                throw new ArgumentException($"{this} is not shared to {connection}");
        }

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
        IComponent component = GetComponent<T>();

        action((component as T)!);

        ChangeComponent(component, null);
    }

    public void RemoveComponent<T>() where T : IComponent => RemoveComponent<T>(null);

    public void Send(IEvent @event) {
        lock (SharedPlayers) {
            foreach (PlayerConnection playerConnection in SharedPlayers)
                playerConnection.Send(new SendEventCommand(@event, this));
        }
    }

    public IEntity Clone() {
        if (!TemplateAccessor.HasValue)
            return new Entity(Id, null, Components.ToHashSet());

        TemplateAccessor templateAccessor = TemplateAccessor.Value;
        templateAccessor = new TemplateAccessor(templateAccessor.Template, templateAccessor.ConfigPath);

        return new Entity(Id, templateAccessor, Components.ToHashSet());
    }

    public void AddComponent(IComponent component, PlayerConnection? excluded) {
        Type type = component.GetType();

        lock (TypeToComponent) {
            if (TypeToComponent.ContainsKey(type))
                throw new ArgumentException($"{this} already has component {type}");

            TypeToComponent[type] = component;
        }

        Logger.Debug("Added {Type} component to the {Entity}", type, this);

        lock (SharedPlayers) {
            foreach (PlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
                playerConnection.Send(new ComponentAddCommand(this, component));
        }
    }

    public void ChangeComponent(IComponent component, PlayerConnection? excluded) {
        Type type = component.GetType();

        lock (TypeToComponent) {
            if (!TypeToComponent.ContainsKey(type))
                throw new ArgumentException($"{this} does not have component {type}");

            TypeToComponent[type] = component;
        }

        lock (SharedPlayers) {
            foreach (PlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
                playerConnection.Send(new ComponentChangeCommand(this, component));
        }
    }

    public void RemoveComponent<T>(PlayerConnection? excluded) where T : IComponent {
        Type type = typeof(T);

        lock (TypeToComponent) {
            if (!TypeToComponent.Remove(type))
                throw new ArgumentException($"{this} does not have component {typeof(T)}");
        }

        lock (SharedPlayers) {
            foreach (PlayerConnection playerConnection in SharedPlayers.Where(pc => pc != excluded))
                playerConnection.Send(new ComponentRemoveCommand(this, type));
        }
    }

    public override string ToString() =>
        $"{GetType().Name} {{ Id: {Id}; TemplateAccessor: {TemplateAccessor}; Components {{ {string.Join(", ", Components.Select(c => c.GetType().Name))} }} }}";
}
