using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Serilog;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Commands;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public class ComponentAddCommand(
    IEntity entity,
    IComponent component
) : EntityCommand(entity) {
    [ProtocolVaried, ProtocolPosition(1)] public IComponent Component { get; private set; } = component;

    public override async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider) {
        ILogger logger = connection.Logger.ForType<ComponentAddCommand>();
        Type type = Component.GetType();
        ClientAddableAttribute? clientAddable = type.GetCustomAttribute<ClientAddableAttribute>();

        if (clientAddable == null) {
            logger.Error("{Component} is not in whitelist ({Entity})", type.Name, Entity);
            /*ChatUtils.SendMessage($"ClientAddable: {type.Name}", ChatUtils.GetChat(connection), [connection], null);*/
            return; // maybe disconnect
        }

        await Entity.AddComponent(Component, connection);
        await Component.Added(connection, Entity);

        logger.Debug("Added {Component} to {Entity}", type.Name, Entity);
    }

    public override string ToString() =>
        $"ComponentAdd command {{ Entity: {Entity}, Component: {Component.GetType().Name} }}";
}
