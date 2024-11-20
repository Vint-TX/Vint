using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Items.Module;

[ProtocolId(1485504324992)]
public class ModuleAssembleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) =>
        await connection.AssembleModule(entities.First());
}
