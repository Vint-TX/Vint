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

    public void Share(IPlayerConnection connection);

    public void Unshare(IPlayerConnection connection);

    public void AddComponent(IComponent component, IPlayerConnection? excluded = null);

    public void AddComponent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new();

    public void AddComponent<T>(string configPath, IPlayerConnection? excluded = null) where T : class, IComponent;

    public void AddGroupComponent<T>(IEntity? entity = null, IPlayerConnection? excluded = null) where T : GroupComponent;

    public void AddComponentFrom<T>(IEntity entity, IPlayerConnection? excluded = null) where T : class, IComponent;

    public void AddComponentIfAbsent(IComponent component, IPlayerConnection? excluded = null);

    public void AddComponentIfAbsent<T>(IPlayerConnection? excluded = null) where T : class, IComponent, new();

    public bool HasComponent(IComponent component);

    public bool HasComponent<T>() where T : class, IComponent;

    public bool HasComponent(Type type);

    public T GetComponent<T>() where T : class, IComponent;

    public IComponent GetComponent(Type type);

    public void ChangeComponent<T>(Action<T> action) where T : class, IComponent;

    public void ChangeComponent(IComponent component, IPlayerConnection? excluded = null);

    public void RemoveComponent<T>(IPlayerConnection? excluded = null) where T : class, IComponent;

    public void RemoveComponent(IComponent component, IPlayerConnection? excluded = null);

    public void RemoveComponent(Type type, IPlayerConnection? excluded = null);

    public void RemoveComponentIfPresent<T>(IPlayerConnection? excluded = null) where T : class, IComponent;

    public void RemoveComponentIfPresent(IComponent component, IPlayerConnection? excluded = null);

    public void RemoveComponentIfPresent(Type type, IPlayerConnection? excluded = null);

    public IEntity Clone();
}
