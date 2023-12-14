namespace Vint.Core.ECS.Components.Server;

public interface IConvertible<in T> where T : IComponent {
    void Convert(T component);
}