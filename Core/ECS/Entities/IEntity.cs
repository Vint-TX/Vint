using ConcurrentCollections;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Commands;
using Vint.Core.Server;

namespace Vint.Core.ECS.Entities;

public interface IEntity {
    public long Id { get; set; }
    public TemplateAccessor? TemplateAccessor { get; }
    public IEnumerable<IComponent> Components { get; }
    public ConcurrentHashSet<IPlayerConnection> SharedPlayers { get; }

    protected EntityShareCommand ToShareCommand();

    protected EntityUnshareCommand ToUnshareCommand();

    public Task Share(IPlayerConnection connection);

    public Task Unshare(IPlayerConnection connection);

    public Task AddComponent(IComponent component, IPlayerConnection? excluded = null);

    public Task AddComponent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new();

    public Task AddComponent<T>(string configPath, IPlayerConnection? excluded = null) where T : class, IComponent;

    public Task AddGroupComponent<T>(IEntity? entity = null, IPlayerConnection? excluded = null) where T : GroupComponent;

    public Task AddComponentFrom<T>(IEntity entity, IPlayerConnection? excluded = null) where T : class, IComponent;

    public Task AddComponentIfAbsent(IComponent component, IPlayerConnection? excluded = null);

    public Task AddComponentIfAbsent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new();

    public bool HasComponent(IComponent component);

    public bool HasComponent<T>() where T : class, IComponent;

    public bool HasComponent(Type type);

    public T GetComponent<T>() where T : class, IComponent;

    public IComponent GetComponent(Type type);

    public Task ChangeComponent<T>(Func<T, Task> func) where T : class, IComponent;

    public Task ChangeComponent<T>(Action<T> action) where T : class, IComponent;

    public Task ChangeComponent(IComponent component, IPlayerConnection? excluded = null);

    public Task RemoveComponent<T>(IPlayerConnection? excluded = null) where T : class, IComponent;

    public Task RemoveComponent(IComponent component, IPlayerConnection? excluded = null);

    public Task RemoveComponent(Type type, IPlayerConnection? excluded = null);

    public Task RemoveComponentIfPresent<T>(IPlayerConnection? excluded = null) where T : class, IComponent;

    public Task RemoveComponentIfPresent(IComponent component, IPlayerConnection? excluded = null);

    public Task RemoveComponentIfPresent(Type type, IPlayerConnection? excluded = null);

    public IEntity Clone();
}
