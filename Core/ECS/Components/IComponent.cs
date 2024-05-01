using System.Reflection;
using System.Runtime.CompilerServices;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Components;

public interface IComponent {
    public Task Added(IPlayerConnection connection, IEntity entity) => Task.CompletedTask;

    public Task Changed(IPlayerConnection connection, IEntity entity) => Task.CompletedTask;

    public Task Removed(IPlayerConnection connection, IEntity entity) => Task.CompletedTask;

    public IComponent Clone() {
        Type type = GetType();
        ILogger logger = Log.Logger.ForType(type);
        IComponent component = (IComponent)RuntimeHelpers.GetUninitializedObject(type);

        foreach (PropertyInfo property in type.GetProperties()) {
            if (property.SetMethod == null) {
                logger.Warning("Cannot clone {Property} because it does not have set accessor", property.Name);
                continue;
            }

            property.SetValue(component, type.GetProperty(property.Name)!.GetValue(this));
        }

        logger.Verbose("Cloned");
        return component;
    }
}
