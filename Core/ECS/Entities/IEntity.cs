using ConcurrentCollections;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Commands;

namespace Vint.Core.ECS.Entities;

public interface IEntity {
    long Id { get; set; }
    TemplateAccessor? TemplateAccessor { get; }
    IEnumerable<IComponent> SortedComponents { get; }
    IEnumerable<IComponent> Components { get; }
    ConcurrentHashSet<IPlayerConnection> SharedPlayers { get; }

    protected EntityShareCommand ToShareCommand();

    protected EntityUnshareCommand ToUnshareCommand();

    Task Share(IPlayerConnection connection);

    Task Unshare(IPlayerConnection connection);

    Task AddComponent(IComponent component, IPlayerConnection? excluded = null);

    Task AddComponent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new();

    Task AddComponent<T>(string configPath, IPlayerConnection? excluded = null) where T : class, IComponent;

    Task AddGroupComponent<T>(IEntity? key = null, IPlayerConnection? excluded = null) where T : GroupComponent;

    Task AddComponentFrom<T>(IEntity entity, IPlayerConnection? excluded = null) where T : class, IComponent;

    Task AddComponentFromConfig<T>() where T : class, IComponent;

    Task AddComponentIfAbsent(IComponent component, IPlayerConnection? excluded = null);

    Task AddComponentIfAbsent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new();

    bool HasComponent(IComponent component);

    bool HasComponent<T>() where T : class, IComponent;

    bool HasComponent(Type type);

    T GetComponent<T>() where T : class, IComponent;

    IComponent GetComponent(Type type);

    Task ChangeComponent<T>(Func<T, Task> func) where T : class, IComponent;

    Task ChangeComponent<T>(Action<T> action) where T : class, IComponent;

    Task ChangeComponent(IComponent component, IPlayerConnection? excluded = null);

    Task RemoveComponent<T>(IPlayerConnection? excluded = null) where T : class, IComponent;

    Task RemoveComponent(IComponent component, IPlayerConnection? excluded = null);

    Task RemoveComponent(Type type, IPlayerConnection? excluded = null);

    Task RemoveComponentIfPresent<T>(IPlayerConnection? excluded = null) where T : class, IComponent;

    Task RemoveComponentIfPresent(IComponent component, IPlayerConnection? excluded = null);

    Task RemoveComponentIfPresent(Type type, IPlayerConnection? excluded = null);

    IEntity Clone();
}
