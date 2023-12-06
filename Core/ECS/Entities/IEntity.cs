using Vint.Core.ECS.Components;
using Vint.Core.ECS.Events;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Commands;
using Vint.Core.Server;

namespace Vint.Core.ECS.Entities;

public interface IEntity {
    public long Id { get; }
    public TemplateAccessor? TemplateAccessor { get; }
    public HashSet<IComponent> Components { get; }

    protected EntityShareCommand ToShareCommand();

    protected EntityUnshareCommand ToUnshareCommand();

    public void Share(PlayerConnection connection);

    public void Unshare(PlayerConnection connection);

    public void AddComponent(IComponent component);

    public bool HasComponent<T>() where T : IComponent;

    public T GetComponent<T>() where T : class, IComponent;

    public void ChangeComponent<T>(Action<T> action) where T : class, IComponent;

    public void RemoveComponent<T>() where T : IComponent;

    public void Send(IEvent @event);

    public IEntity Clone();
}

public interface IInternalEntity {
    public void AddComponent(IComponent component, PlayerConnection? excluded = null);

    public void ChangeComponent(IComponent component, PlayerConnection? excluded = null);

    public void RemoveComponent<T>(PlayerConnection? excluded = null) where T : IComponent;
}
