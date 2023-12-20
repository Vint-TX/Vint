using Vint.Core.ECS.Components;
using Vint.Core.ECS.Events;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Commands;
using Vint.Core.Server;

namespace Vint.Core.ECS.Entities;

public interface IEntity {
    public long Id { get; set; }
    public TemplateAccessor? TemplateAccessor { get; }
    public IEnumerable<IComponent> Components { get; }

    protected EntityShareCommand ToShareCommand();

    protected EntityUnshareCommand ToUnshareCommand();

    public void Share(IPlayerConnection connection);

    public void Unshare(IPlayerConnection connection);

    public void AddComponent(IComponent component);

    public bool HasComponent<T>() where T : IComponent;

    public T GetComponent<T>() where T : class, IComponent;

    public void ChangeComponent<T>(Action<T> action) where T : class, IComponent;

    public void RemoveComponent<T>() where T : IComponent;

    public void Send(IEvent @event);

    public IEntity Clone();
}

public interface IInternalEntity {
    public void AddComponent(IComponent component, IPlayerConnection? excluded = null);

    public void ChangeComponent(IComponent component, IPlayerConnection? excluded = null);

    public void RemoveComponent<T>(IPlayerConnection? excluded = null) where T : IComponent;
}